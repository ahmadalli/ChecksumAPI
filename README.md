# Checksum API

This project is for calculating checksum of a url.

## How to use api

call `/` via get method.

### Parameters

- `fileUrl` is url of file.
- `offsetPercent` is files have some metadata like id3 tags. in order to prevent them to interfere with core data of file, I'm using offset percent. when you use that, some parts of beginning and ending of the file will be ignored.
- `algorithm` is hashing algorithm. The default value is `MD5`. You can find acceptable values [here](https://github.com/dotnet/corefx/blob/master/src/System.Security.Cryptography.Algorithms/src/System/Security/Cryptography/CryptoConfig.cs).
- `force` is for forcing the api to recalculate checksum of a file that has been already calculated. The dafault value is `false`.
