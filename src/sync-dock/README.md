# Folder Sync Client

- Dump files into the given directory, making sure to use dummy filename / extension and rename after the copy is complete.
- Put in a JSON file with the same basename, containing a `sessionid`.
- When an upload is complete, files are moved to `archive`.
- Files not uploaded are picked up when the library starts.
- Files are uploaded one at a time.