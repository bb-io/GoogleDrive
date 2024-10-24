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

- **Download file** Download a specific file from Google Drive.
- **Upload file** Upload a file to Google Drive.
- **Delete item** Delete a file from Google Drive.
- **Create folder** Create a folder in Google Drive.
- **Search files** Search files in Google Drive by providing specific search criteria (e.g. file name, file type, etc.). Returns all files that match the search criteria.
- **Find file information** Similar to the search files action, but this action returns only the first file that matches the search criteria so you don't need to use a loop to get the file information. Also returns `Is found` output to indicate if the file was found or not.

### Labels

- **Get file labels** returns all the label field keys attached to a file. This is in the form of a list of strings. You can use the array contains utility action to verify if certain labels are added to this file.

## Events

- **On items added** This event triggers when file or folder is added to Google Drive. Inputs allows to specify the folder to watch.
- **On items updated** This event triggers when file or folder is updated in Google Drive. Inputs allows to specify the folder to watch.
- **On items removed** This event triggers when file or folder is deleted from Google Drive. Inputs allows to specify the folder to watch.
- **On items restored** This event triggers when file or folder is restored from trash in Google Drive. Inputs allows to specify the folder to watch.
- **On files created** This event triggers when any new files are created.
- **On files updated** This event triggers when any existing files are updated.
- **On files deleted in shared drives** This event triggers when any existing files are deleted in the shared drives.

## Example 

Here is an example of how you can use the Google Drive app in a workflow:

![example](image/README/example.png)

In this example, the workflow starts with the **On items added** event, which triggers when any file or folder is added to Google Drive. Then, the workflow uses the **Download files** action to download the file that was added. In the next step we translate the file via `DeepL` and then upload the translated file to Slack channel.

## Eggs

Check downloadable workflow prototypes featuring this app that you can import to your Nests [here](https://docs.blackbird.io/eggs/storage-to-mt/). 

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
