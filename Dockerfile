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

EXPOSE 5000

RUN sh -c "echo \"{}\" > appsettings.json"
ENTRYPOINT [ "sh", "docker_start.sh" ]
