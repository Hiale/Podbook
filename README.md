# Podbook

A small application which helps to import a bunch of MP3 files into a podcast application. This can be handy if you have an audiobook which is split in multiple files and want to have advantages of modern podcast player applications such as custom playback speed.
Some podcast player applications support file system import of media files, in this case Podbook is not necessary. In other cases, this applications provides temporary HTTP server which serves a podcast feed and the actual media files.

## Requirements

* .NET Core Runtime v2.0+
* Basic network know-how

## How to Use

1. Have all your media files in a single directory.
2. Open cmd and change to the directory in which this application was extracted.
3. Launch **dotnet Podbook**
4. You are requested for the path of the media files, enter (or paste) the path and press Enter
5. You are requested for the host name or IP address of your local machine. This host name must be accessible from your podcast player application. Your are provided with a bunch of options to select from. To select one, just type the number and press Enter. If none of the options is suitable for you, it is possible to enter a custom host name.
6. A HTTP server is started and an address is printed, enter this in your podcast player. If everything works, you should see all your media files as episodes.
7. Download all episodes.
8. Press a key, the server will shut down.
9. Enjoy your episodes!

## Command-line interface

Podbook supports a command-line interface.

#### Parameters
* *-path*
  - Directory which contains the episodes.
* *-host*
  - Host name or IP of this machine.
* *-port*
  - Port, which should be used to connect to this program. If none is provided, port 80 is tried and if that one is used by another application, a random free port is chosen.

#### Examples
* *podbook -path C:\audiobooks\1983*
* *podbook -path "H:\my Audiobooks\Journey" -port 1234*

## Future improvements

* More media types like Ogg
* Image in Feed

## Credit

* [TagLib#](https://github.com/mono/taglib-sharp)
* [SimpleHttpServer](https://github.com/jeske/SimpleHttpServer)

## License

MIT