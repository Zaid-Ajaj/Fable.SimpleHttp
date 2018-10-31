@echo off

if exist paket.lock (
    .paket\paket.exe restore
) else (
    .paket\paket.exe install
)

if errorlevel 1 (
  exit /b %errorlevel%
)

packages\build\FAKE\tools\FAKE.exe build.fsx %*