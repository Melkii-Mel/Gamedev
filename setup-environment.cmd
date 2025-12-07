@echo off
cd /d %~dp0
start "" wt -d .
start "" rider64 "%~dp0\gamedev.sln"
start "" explorer .
start "" obsidian.exe