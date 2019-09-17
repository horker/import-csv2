task . Compile, Build, ImportDebug, Test

Set-StrictMode -Version Latest

############################################################
# Settings
############################################################

$SOURCE_PATH = "$PSScriptRoot\source"
$SCRIPT_PATH = "$PSScriptRoot\scripts"

$MODULE_PATH = "$PSScriptRoot\module\pscsvhelper"
$MODULE_PATH_DEBUG = "$PSScriptRoot\module\debug\pscsvhelper"

$SOLUTION_FILE = "$PSScriptRoot\source\Horker.CsvHelper.sln"

$OBJECT_FILES = @(
    "CsvHelper.dll"
    "Horker.CsvHelper.dll"
    "Horker.CsvHelper.pdb"
    "Horker.CsvHelper.PowerShell.dll"
    "Horker.CsvHelper.PowerShell.pdb"
)

$HELP_XML = "$PSScriptRoot\docs\Horker.CsvHelper.PowerShell.dll-Help.xml"

#TODO
#$LIBRARY_PATH = "

#$TEMPLATE_INPUT_PATH = "$PSScriptRoot\templates"
#$TEMPLATE_OUTPUT_PATH = "$PSScriptRoot\source\Horker.PSCNTK\Generated files"

#$HELP_INPUT =  "$SOURCE_PATH\bin\Release\Horker.Math.dll"
#$HELP_INTERM = "$SOURCE_PATH\bin\Release\Horker.Data.dll-Help.xml"
#$HELP_OUTPUT = "$MODULE_PATH\Horker.Data.dll-Help.xml"
#$HELPGEN = "$PSScriptRoot\vendor\XmlDoc2CmdletDoc.0.2.10\tools\XmlDoc2CmdletDoc.exe"

############################################################
# Helper cmdlets
############################################################

function New-Folder2 {
  param(
    [string]$Path
  )

  try {
    $null = New-Item -Type Directory $Path -EA Stop
    Write-Host -ForegroundColor DarkCyan "$Path created"
  }
  catch {
    Write-Host -ForegroundColor DarkYellow $_
  }
}

function Copy-Item2 {
  param(
    [string]$Source,
    [string]$Dest
  )

  try {
    Copy-Item $Source $Dest -EA Stop
    Write-Host -ForegroundColor DarkCyan "Copy from $Source to $Dest done"
  }
  catch {
    Write-Host -ForegroundColor DarkYellow $_
  }
}

function Remove-Item2 {
  param(
    [string]$Path
  )

  Resolve-Path $PATH | foreach {
    try {
      Remove-Item $_ -EA Stop -Recurse -Force
      Write-Host -ForegroundColor DarkCyan "$_ removed"
    }
    catch {
      Write-Host -ForegroundColor DarkYellow $_
    }
  }
}

############################################################
# Tasks
############################################################

task Compile {
  msbuild $SOLUTION_FILE /p:Configuration=Debug /nologo /v:minimal
  msbuild $SOLUTION_FILE /p:Configuration=Release /nologo /v:minimal
}

task Build {
  . {
    $ErrorActionPreference = "Continue"

    function Copy-ObjectFiles {
      param(
        [string]$targetPath,
        [string]$objectPath
      )

      New-Folder2 $targetPath

      Copy-Item2 "$SCRIPT_PATH\*" $targetPath
      $OBJECT_FILES | foreach {
        $path = Join-Path $objectPath $_
        Copy-Item2 $path $targetPath
      }
    }

    Copy-ObjectFiles $MODULE_PATH "$SOURCE_PATH\bin\Release"
    Copy-ObjectFiles $MODULE_PATH_DEBUG "$SOURCE_PATH\bin\Debug"

    Copy-Item2 $HELP_XML $MODULE_PATH
    Copy-Item2 $HELP_XML $MODULE_PATH_DEBUG
  }
}

task UpdateHelp {
    Update-MarkdownHelp docs\
}

task CompileHelp {
    New-ExternalHelp docs\ -outputpath docs\ -force
}

task Test Build, ImportDebug, {
  Invoke-Pester "$PSScriptRoot\tests"
}

task ImportDebug {
    Import-Module $MODULE_PATH_DEBUG -Force
}

task Clean {
  Remove-Item2 "$MODULE_PATH\*"
  Remove-Item2 "$MODULE_PATH_DEBUG\*"
}

task Pack {
    nuget.exe pack source\Horker.CsvHelper\Horker.CsvHelper.csproj -Prop Configuration=Release -Symbol -OutputDirectory nuget\
}
