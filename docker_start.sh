#!/bin/bash
dotnet WakaBot.Web.dll -- --urls http://0.0.0.0:5000 || exit 1
