#!/bin/bash
dotnet publish -c Release --self-contained -r linux-x64 --framework net10.0 -o apps
rm -rf apps/appsettings.Development.json
rm -rf apps/appsettings.secrets.json
