FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env

# https://askubuntu.com/questions/651441/how-to-install-arial-font-and-other-windows-fonts-in-ubuntu#651442
RUN apt-get update
RUN apt-get install -y cabextract
RUN wget https://www.freedesktop.org/software/fontconfig/webfonts/webfonts.tar.gz
RUN tar -xzf webfonts.tar.gz
RUN mkdir /fonts
RUN cd msfonts/ && \
  cabextract *.exe && \
  cp *.ttf *.TTF /fonts/

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
COPY --from=build-env /fonts .local/share/

EXPOSE 5000
ENV ASPNETCORE_URLS https://0.0.0.0:5000
ENTRYPOINT [ "sh", "docker_start.sh" ]
