<#!
.SYNOPSIS
    Append a collaboration tip entry.

.DESCRIPTION
    Writes a topic block to docs/CollaborationAndDebugTips.txt. Search the file for an
    existing topic before running. If the topic already exists, append new commit
    references or observations to that block rather than creating a duplicate.

.EXAMPLE
    pwsh tools/add-tip.ps1 -Topic "FTP service fixes" -Observations "Clarified null guard" -Refs "b6b4e9d"
#>
param(
  [Parameter(Mandatory=$true)][string]$Topic,
  [Parameter(Mandatory=$true)][string]$Observations,
  [string]$Limitations = "",
  [string]$Prompts = "",
  [string]$Decisions = "",
  [string]$Actions = "",
  [string]$Refs = ""
)
$path = Join-Path $PSScriptRoot "..\docs\CollaborationAndDebugTips.txt"
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm"
$block = @"
[$timestamp] Topic: $Topic
Context: (summarize the scenario)
Observations: $Observations
Codex Limitations noticed: $Limitations
Effective Prompts / Instructions that worked: $Prompts
Decisions & Rationale: $Decisions
Action Items: $Actions
Related Commits/PRs: $Refs
"@
Add-Content -Path $path -Value $block

