# Blackbird.io Google Drive

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Google Drive is a file-hosting service and synchronization service developed by Google. Launched on April 24, 2012, Google Drive allows users to store files in the cloud (on Google's servers), synchronize files across devices, and share files

## Before setting up

Before you can connect you need to make sure that:

- You have a **Google Drive** account and you have the credentials to access it.

## Connecting

1. Navigate to Apps, and identify the **Google Drive** app. You can use search to find it.
2. Click _Add Connection_.
3. Name your connection for future reference e.g. 'Google Drive connection'.
4. Click _Authorize connection_.
5. Establish Google Drive connection via OAuth.

![connection](image/README/connection.png)

## Actions

### Files

- **Download file** Download a file by ID.
- **Upload file** Upload a file to a selected folder.
    Advanced settings:
    - **Save as** Choose a MIME type to save the uploaded file as.
- **Delete item** Delete a file or folder by ID.
- **Search files** Search files by folder, name, and MIME type filters. Outputs all matching files and the total count.
    Advanced settings:
    - **Folder ID** Limit results to files in a specific folder.
    - **Include subfolders?** Include files from subfolders of the selected folder.
    - **Max subfolder level to search** Set how many subfolder levels to include.
    - **File name** Limit results to files whose names contain this text.
    - **File name must be exact match?** Match only files with an exact file name.
    - **Mime type** Limit results to files with this MIME type.
    - **Limit** Set the maximum number of files to output.
- **Get file information** Get the first file that matches the search criteria and output whether a file was found.
- **Update file** Update file metadata or content without changing the file ID.
    Advanced settings:
    - **New file content** Replace the file content.
    - **New file name** Rename the file.

### Folders

- **Create folder** Create a folder in a selected parent folder.
- **Check folder exists** Check whether a folder with a specific name exists in a parent folder.
- **Get folder information** Get folder details by folder ID.

### Labels

- **Get file labels** Get all label field keys attached to a file.

## Events

### File events

- **On files deleted in shared drives** On files deleted in shared drives.
- **On files created** On files created in a specified folder.
    Advanced settings:
    - **File name contains** Trigger only for files whose names contain this text.
    - **Mime type** Trigger only for files with this MIME type.
- **On files updated** On files updated in a specified folder.
    Advanced settings:
    - **Folder ID** Trigger only for files in this folder.
    - **File ID** Trigger only for a specific file.

## Example 

Here is an example of how you can use the Google Drive app in a workflow:

![example](image/README/example.png)

In this example, the workflow starts with the **On files created** event. Then, the workflow uses the **Download file** action to download the created file. In the next step, the file is translated via `DeepL` and uploaded to a Slack channel.

## Eggs

Check downloadable workflow prototypes featuring this app that you can import to your Nests [here](https://docs.blackbird.io/eggs/storage-to-mt/). 

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
