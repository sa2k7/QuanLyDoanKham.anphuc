# manage-sandbox.ps1 - Quản lý Git Worktree cho Solo-Ninja
# Cách dùng: .\manage-sandbox.ps1 create <name> | delete <name> | list

param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("create", "delete", "list")]
    $Action,
    
    [Parameter(Mandatory=$false)]
    $Name
)

$RootDir = Get-Location
$SandboxDir = Join-Path $RootDir.Parent.FullName "sandboxes"

if (-not (Test-Path $SandboxDir)) {
    New-Item -ItemType Directory -Path $SandboxDir | Out-Null
    Write-Host "Created sandboxes directory at $SandboxDir" -ForegroundColor Cyan
}

switch ($Action) {
    "create" {
        if (-not $Name) { Write-Error "Tên sandbox không được để trống"; break }
        $Target = Join-Path $SandboxDir $Name
        Write-Host "Creating sandbox: $Name at $Target..." -ForegroundColor Green
        
        # Tạo worktree mới từ nhánh hiện tại
        git worktree add -b "sandbox/$Name" $Target
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully created sandbox $Name." -ForegroundColor Green
            Write-Host "Tip: Cd into $Target and start building." -ForegroundColor Yellow
        }
    }
    
    "delete" {
        if (-not $Name) { Write-Error "Tên sandbox không được để trống"; break }
        Write-Host "Deleting sandbox: $Name..." -ForegroundColor Red
        git worktree remove $Name --force
        git branch -D "sandbox/$Name"
    }
    
    "list" {
        Write-Host "Current active sandboxes:" -ForegroundColor Cyan
        git worktree list
    }
}
