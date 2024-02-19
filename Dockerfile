FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
RUN mkdir /app

COPY . /app
WORKDIR /app/WakaBot.Web
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:6.0
RUN mkdir /app

WORKDIR /app/

COPY docker_start.sh .
RUN chmod -R 600 .
COPY --from=build-env /app/out .
RUN chmod u+x docker_start.sh

EXPOSE 5000

# From https://learn.microsoft.com/en-us/dotnet/core/docker/build-container?tabs=linux&pivots=dotnet-8-0
# > Before .NET 8, containers configured to run as read-only may fail with Failed to create CoreCLR, HRESULT: 0x8007000E. To address this issue, specify a DOTNET_EnableDiagnostics environment variable as 0 (just before the ENTRYPOINT step):
ENV DOTNET_EnableDiagnostics=0

RUN sh -c "echo \"{}\" > appsettings.json"
ENTRYPOINT [ "docker_start.sh" ]
