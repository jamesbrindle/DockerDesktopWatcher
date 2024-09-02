# Docker Desktop Watcher

A simple C# Windows service or standalone app to use in a scheduled task to restart Docker Desktop if it's not responding.

## Installing the Service

Run with PowerShell as admin:

```
New-Service -Name "DockerDesktopWatcher" -BinaryPathName "<folder path>\DockerDesktopWatcher.Service.exe" -StartupType Automatic
```

## Restart Service Automatically on Failure

```
sc.exe failure "DockerDesktopWatcher" reset= 86400 actions= restart/5000
```

## Uninstalling the Servvvice

```
sc.exe delete "DockerDesktopWatcher"
```