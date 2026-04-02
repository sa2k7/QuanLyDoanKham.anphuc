# Script: Export Project Context for AI
# Usage: Run in PowerShell from root folder: ./tools/export-context.ps1

$outputFile = "PROJECT_SNAPSHOT.txt"
$rootPath = (Get-Location).Path
$excludeFolders = @(".git", "node_modules", "bin", "obj", ".gemini", ".next", "dist", "Migrations")
$includeExtensions = @(".cs", ".vue", ".js", ".ts", ".md", ".json")

Write-Host "--- Packaging Project Context for AI ---" -ForegroundColor Cyan

# 0. Khởi tạo file trắng với Encoding chuẩn UTF8
$null | Set-Content -Path $outputFile -Encoding utf8

# 1. Project Directory Structure
Add-Content -Path $outputFile -Value "### PROJECT STRUCTURE ###"
Get-ChildItem -Recurse | Where-Object { $excludeFolders -notcontains $_.Name } | Select-Object FullName | ForEach-Object { 
    $relPath = $_.FullName.Replace($rootPath, "")
    Add-Content -Path $outputFile -Value $relPath
}
Add-Content -Path $outputFile -Value "`n"

# 2. Rules & Guidelines
Add-Content -Path $outputFile -Value "### AGENT IDENTITY & RULES ###"
if (Test-Path ".agent/rules") {
    Get-ChildItem ".agent/rules/*.md" | ForEach-Object {
        $fileName = $_.Name
        Add-Content -Path $outputFile -Value "FILE: $fileName"
        Add-Content -Path $outputFile -Value "---"
        Get-Content $_.FullName | Add-Content -Path $outputFile
        Add-Content -Path $outputFile -Value "---`n"
    }
}

# 3. Domain Logic & Core Files
Add-Content -Path $outputFile -Value "### CORE CODE & LOGIC ###"
Get-ChildItem -Recurse | Where-Object { 
    $_.PSIsContainer -eq $false -and 
    $includeExtensions -contains $_.Extension -and
    $_.FullName -notmatch "\\(bin|obj|node_modules|Migrations)\\"
} | ForEach-Object {
    $currentRelPath = $_.FullName.Replace($rootPath, "")
    Add-Content -Path $outputFile -Value "FILE: $currentRelPath"
    Add-Content -Path $outputFile -Value "---"
    Get-Content $_.FullName | Add-Content -Path $outputFile
    Add-Content -Path $outputFile -Value "---`n"
}

Write-Host "Success! context saved to $outputFile" -ForegroundColor Green
Write-Host "Copy its content to ChatGPT or Claude to give them full context." -ForegroundColor Yellow
