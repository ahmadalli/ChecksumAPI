# Checksum API

This project is for calculating checksum of a url. For now, MD5 hashing is implemented.

## MD5

To use this api call `/MD5`

### Parameters

- `fileUrl`: url of file.
- `offsetPercent`: files have some metadata like id3 tags. in order to prevent them to interfere with core data of file, I'm using offset percent. when you use that, some parts of beginning and ending of the file will be ignored. You also can set this on `appsettings.json` file.