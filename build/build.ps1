$baseDir = Resolve-Path(".")
$outputFolder = Join-Path $baseDir "_build_output\"
$sourceDir = Join-Path $baseDir "source"
$solution = Join-Path $sourceDir "Changes.sln"
$windir = $env:windir

if((ls "$windir\Microsoft.NET\Framework\v4.0*") -eq $null ) {
	throw "Building requires .NET 4.0, which doesn't appear to be installed on this machine."
}

$v4_net_version = (ls "$windir\Microsoft.NET\Framework\v4.0*").Name

$msbuild = "$windir\Microsoft.NET\Framework\$v4_net_version\MSBuild.exe"

$options = "/m /noconsolelogger /p:Configuration=Release"

if (Test-Path $outputFolder) {
	Remove-Item $outputFolder -Recurse -Force
}

$build = "$msbuild ""$solution"" $options /t:Build"

Invoke-Expression $build

$nuget = Join-Path $baseDir "tools\NuGet\nuget.exe"

$project = Join-Path $baseDir "source\Changes\Changes.csproj"

$package = "$nuget pack $project -Properties ""Configuration=Release"""

Invoke-Expression $package
