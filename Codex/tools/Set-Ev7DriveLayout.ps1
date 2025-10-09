[CmdletBinding()]
param()

function Get-FreeDriveLetter {
    [CmdletBinding()]
    param(
        [char[]]$Preferred = @(),
        [char[]]$Reserved = @()
    )

    $preferredLetters = @()
    foreach ($letter in $Preferred) {
        if (-not [string]::IsNullOrEmpty($letter)) {
            $preferredLetters += [char]([char]::ToUpperInvariant($letter))
        }
    }

    $reservedLetters = @()
    foreach ($letter in $Reserved) {
        if (-not [string]::IsNullOrEmpty($letter)) {
            $reservedLetters += [char]([char]::ToUpperInvariant($letter))
        }
    }

    $occupiedLetters = @()
    $volumes = Get-Volume -ErrorAction SilentlyContinue | Where-Object { $_.DriveLetter }
    foreach ($volume in $volumes) {
        $occupiedLetters += [char]([char]::ToUpperInvariant($volume.DriveLetter))
    }

    $candidates = @()
    if ($preferredLetters.Count -gt 0) {
        $candidates += $preferredLetters
    }

    $alphabet = @()
    for ($code = [int][char]'D'; $code -le [int][char]'Z'; $code++) {
        $alphabet += [char]$code
    }
    $candidates += $alphabet

    foreach ($candidate in $candidates) {
        if ($candidate -and $candidate -notin $occupiedLetters -and $candidate -notin $reservedLetters) {
            return $candidate
        }
    }

    throw 'No free drive letters are available to complete the mapping.'
}

function Get-PrimaryPartition {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [Microsoft.Management.Infrastructure.CimInstance]$Disk
    )

    $partitions = Get-Partition -DiskNumber $Disk.Number -ErrorAction SilentlyContinue |
        Where-Object { $_.Type -ne 'Reserved' }

    if (-not $partitions) {
        Initialize-Disk -Number $Disk.Number -PartitionStyle GPT -ErrorAction Stop | Out-Null
        $newPartition = New-Partition -DiskNumber $Disk.Number -UseMaximumSize -AssignDriveLetter -ErrorAction Stop
        Format-Volume -Partition $newPartition -FileSystem NTFS -NewFileSystemLabel 'Data' -Confirm:$false -ErrorAction Stop | Out-Null
        return $newPartition
    }

    return $partitions | Sort-Object Size -Descending | Select-Object -First 1
}

function Ensure-DriveLetter {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [Microsoft.Management.Infrastructure.CimInstance]$Partition,

        [Parameter(Mandatory)]
        [char]$Letter,

        [char[]]$ReservedLetters = @()
    )

    $targetLetter = [char]([char]::ToUpperInvariant($Letter))
    $currentLetter = if ($Partition.DriveLetter) { [char]([char]::ToUpperInvariant($Partition.DriveLetter)) } else { $null }

    if ($currentLetter -eq $targetLetter) {
        return
    }

    $existing = $null
    try {
        $existing = Get-Partition -DriveLetter $targetLetter -ErrorAction Stop
    } catch {
        $existing = $null
    }

    if ($existing -and $existing.UniqueId -ne $Partition.UniqueId) {
        $temporaryLetter = Get-FreeDriveLetter -Preferred @('Z','Y','X','W','V') -Reserved @($ReservedLetters + $targetLetter)
        Set-Partition -InputObject $existing -NewDriveLetter $temporaryLetter -ErrorAction Stop | Out-Null
    }

    Set-Partition -InputObject $Partition -NewDriveLetter $targetLetter -ErrorAction Stop | Out-Null
}

$disks = Get-Disk -ErrorAction SilentlyContinue | Where-Object { $_.OperationalStatus -eq 'Online' }
if (-not $disks) {
    Write-Error 'No online disks were detected.'
    exit 1
}

$sataDisks = $disks | Where-Object { $_.BusType -eq 'SATA' } | Sort-Object Number
$usbDisks = $disks | Where-Object { $_.BusType -eq 'USB' } | Sort-Object Number

$primarySata = $null
$secondarySata = $null
$sataPartitions = @()
foreach ($disk in $sataDisks) {
    $partition = Get-PrimaryPartition -Disk $disk
    if ($partition) {
        $sataPartitions += $partition
    }
}

if ($sataPartitions.Count -gt 0) {
    $primarySata = $sataPartitions[0]
}
if ($sataPartitions.Count -gt 1) {
    $secondarySata = $sataPartitions[1]
}

$usbPartition = $null
foreach ($disk in $usbDisks) {
    $candidate = Get-Partition -DiskNumber $disk.Number -ErrorAction SilentlyContinue |
        Where-Object { $_.DriveLetter } |
        Sort-Object Size -Descending |
        Select-Object -First 1
    if ($candidate) {
        $usbPartition = $candidate
        break
    }
}

$plannedAssignments = @()
$reservedLetters = @()

if ($primarySata) {
    $plannedAssignments += [pscustomobject]@{ Partition = $primarySata; Letter = 'D' }
    $reservedLetters += 'D'
}

if ($secondarySata) {
    $plannedAssignments += [pscustomobject]@{ Partition = $secondarySata; Letter = 'E' }
    $reservedLetters += 'E'
}

if ($usbPartition) {
    $usbPreferred = if ($secondarySata) { 'F' } else { 'E' }
    $plannedAssignments += [pscustomobject]@{ Partition = $usbPartition; Letter = $usbPreferred }
    $reservedLetters += $usbPreferred
}

if ($plannedAssignments.Count -eq 0) {
    Write-Warning 'No qualifying SATA or USB partitions were found for reassignment.'
    exit 0
}

foreach ($assignment in $plannedAssignments) {
    $currentLetter = if ($assignment.Partition.DriveLetter) { "$($assignment.Partition.DriveLetter):" } else { '(no letter)' }
    $diskInfo = $null
    try {
        $diskInfo = Get-Disk -Number $assignment.Partition.DiskNumber -ErrorAction Stop
    } catch {
        $diskInfo = $null
    }
    $diskLabel = if ($diskInfo) { "$($diskInfo.FriendlyName) (Disk $($diskInfo.Number))" } else { "Disk $($assignment.Partition.DiskNumber)" }
    Write-Host ("Assigning {0} from {1} to {2}:" -f $diskLabel, $currentLetter, $assignment.Letter)
    Ensure-DriveLetter -Partition $assignment.Partition -Letter $assignment.Letter -ReservedLetters $reservedLetters
}

Write-Host 'Drive mapping completed successfully.'
