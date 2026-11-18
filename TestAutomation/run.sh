#!/bin/sh

. ./env_setup.sh
dotnet build
sudo -E dotnet run

