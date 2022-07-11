param(
    [Parameter(Mandatory)]
    [ValidateSet('Debug','Release')]
    [System.String]$Target,
    
    [Parameter(Mandatory)]
    [System.String]$TargetPath,
    
    [Parameter(Mandatory)]
    [System.String]$TargetAssembly,

    [Parameter(Mandatory)]
    [System.String]$ValheimPath,

    [Parameter(Mandatory)]
    [System.String]$ProjectPath,
    
    [System.String]$DeployPath
)

# Make sure Get-Location is the script path
Push-Location -Path (Split-Path -Parent $MyInvocation.MyCommand.Path)

# Test some preliminaries
("$TargetPath",
 "$ValheimPath",
 "$(Get-Location)\libraries"
) | % {
    if (!(Test-Path "$_")) {Write-Error -ErrorAction Stop -Message "$_ folder is missing"}
}

# Plugin name without ".dll"
$name = "$TargetAssembly" -Replace('.dll')

# Create the mdb file
$pdb = "$TargetPath\$name.pdb"
if (Test-Path -Path "$pdb") {
    Write-Host "Create mdb file for plugin $name"
    Invoke-Expression "& `"$(Get-Location)\libraries\Debug\pdb2mdb.exe`" `"$TargetPath\$TargetAssembly`""
}

# Main Script
Write-Host "Publishing for $Target from $TargetPath"

if ($Target.Equals("Debug")) {
    if ($DeployPath.Equals("")){
      $DeployPath = "$ValheimPath\BepInEx\plugins"
    }
    $plug = New-Item -Type Directory -Path "$DeployPath\$name" -Force
    Write-Host "Copy $TargetAssembly to $plug"
    Copy-Item -Path "$TargetPath\$name.dll" -Destination "$plug" -Force
    Copy-Item -Path "$TargetPath\$name.pdb" -Destination "$plug" -Force
    Copy-Item -Path "$TargetPath\$name.dll.mdb" -Destination "$plug" -Force
}

if($Target.Equals("Release")) {
    $Package = "Package"
    $PackagePath="$ProjectPath\$Package"

    $TSPackagePath="$PackagePath\ThunderStore"
    ("$TSPackagePath/plugins"
    ) | % {
        if (!(Test-Path "$_")) {
            New-Item -Path "$TSPackagePath" -Name "plugins" -ItemType "directory"
        }
    }

    Write-Host "Packaging for ThunderStore..."
    Copy-Item -Path "$TargetPath\$TargetAssembly" -Destination "$TSPackagePath\plugins\$TargetAssembly" -Force
    Copy-Item -Path "$ProjectPath\README.md" -Destination "$TSPackagePath\README.md" -Force
    Write-Host "Compressing..."
    $ZipPath="$TSPackagePath\$name.zip"
    Compress-Archive -Path "$TSPackagePath\*" -DestinationPath "$ZipPath" -Force
    Write-Host "$ZipPath"

    $NexusPackagePath="$PackagePath\Nexus"
    ("$NexusPackagePath"
    ) | % {
        if (!(Test-Path "$_")) {
            New-Item -Path "$PackagePath" -Name "Nexus" -ItemType "directory"
        }
    }
    Write-Host "Packaging for Nexus..."
    Copy-Item -Path "$TargetPath\$TargetAssembly" -Destination "$NexusPackagePath\$TargetAssembly" -Force
    Write-Host "Compressing..."
    $ZipPath="$NexusPackagePath\$name.zip"
    Compress-Archive -Path "$NexusPackagePath\*" -DestinationPath "$ZipPath" -Force
    Write-Host "$ZipPath"
}

# Pop Location
Pop-Location