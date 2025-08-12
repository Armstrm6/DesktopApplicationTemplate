import io
import json
import os
import subprocess
import zipfile

import requests
from github import Github
from openai import OpenAI

WORKFLOW_NAME = ".NET 8.0 Desktop CI/CD"
BRANCH = "dev"
STATE_FILE = ".codex-selfheal/state.json"


def load_state() -> dict:
    if os.path.exists(STATE_FILE):
        with open(STATE_FILE, "r", encoding="utf-8") as f:
            return json.load(f)
    return {}


def save_state(state: dict) -> None:
    os.makedirs(os.path.dirname(STATE_FILE), exist_ok=True)
    with open(STATE_FILE, "w", encoding="utf-8") as f:
        json.dump(state, f)


def main() -> None:
    token = os.environ.get("GITHUB_TOKEN")
    openai_key = os.environ.get("OPENAI_API_KEY")
    repo_name = os.environ.get("GITHUB_REPOSITORY")
    if not all([token, openai_key, repo_name]):
        print("Required environment variables are missing")
        return

    gh = Github(token)
    repo = gh.get_repo(repo_name)
    workflow = next((wf for wf in repo.get_workflows() if wf.name == WORKFLOW_NAME), None)
    if workflow is None:
        print("Workflow not found")
        return

    runs = workflow.get_runs(branch=BRANCH)
    failed_run = next((r for r in runs if r.conclusion == "failure"), None)
    if failed_run is None:
        print("No failed workflow run found")
        return

    sha = failed_run.head_sha
    state = load_state()
    attempts = state.get(sha, 0)
    if attempts >= 3:
        print("Maximum self-heal attempts reached for this commit")
        return
    state[sha] = attempts + 1
    save_state(state)

    headers = {"Authorization": f"token {token}"}
    logs_resp = requests.get(failed_run.logs_url, headers=headers)
    logs_resp.raise_for_status()
    if logs_resp.headers.get("Content-Type", "").startswith("application/zip"):
        with zipfile.ZipFile(io.BytesIO(logs_resp.content)) as z:
            log_text = "".join(z.read(name).decode("utf-8", errors="ignore") for name in z.namelist())
    else:
        log_text = logs_resp.content.decode("utf-8", errors="ignore")
    log_excerpt = log_text[-15000:]

    prompt = f"""
Repository: {repo.full_name}
Branch: {BRANCH}
Pipeline file: {workflow.path}

Failure Logs:
{log_excerpt}

Return a unified diff patch that fixes the minimal root cause. Add or update regression tests when the failure is functional. Avoid unrelated refactors and preserve public APIs when possible. Ensure 'dotnet build', 'dotnet test', and 'dotnet format --verify-no-changes' pass locally.
"""

    client = OpenAI(api_key=openai_key)
    response = client.chat.completions.create(
        model="gpt-4o",
        temperature=0,
        messages=[{"role": "user", "content": prompt}],
    )
    patch = response.choices[0].message.content.strip()
    if not patch or "diff --git" not in patch:
        print("No valid diff received")
        return

    with open("fix.patch", "w", encoding="utf-8") as f:
        f.write(patch)

    subprocess.run(["git", "apply", "fix.patch"], check=True)
    subprocess.run(["git", "add", "-A"], check=True)
    subprocess.run(
        [
            "git",
            "commit",
            "-m",
            "fix: self-heal pipeline failure (automated by Codex)",
        ],
        check=True,
    )
    subprocess.run(["git", "push", "origin", BRANCH], check=True)


if __name__ == "__main__":
    main()
