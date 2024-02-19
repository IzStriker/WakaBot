#!/bin/bash
dotnet ef database update --context WakaBot.Core.Data.PostgreSqlContext || exit 1
dotnet WakaBot.Web.dll || exit 1
