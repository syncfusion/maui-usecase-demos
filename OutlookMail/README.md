# Fluent Mail

Fluent Mail is a Syncfusion .NET MAUI showcase sample. It presents an Outlook-inspired mail workspace with local mock data and no backend.

## Overview

The sample shows how to combine Syncfusion controls in a mail-style app. It focuses on folders, message lists, reading panes, drafts, archive browsing, deleted mail recovery, and compose flows.

## Controls demonstrated

- SfTreeView for mailbox folders
- SfListView for message, draft, archive, and deleted lists
- SfRichTextEditor for mail composition and draft editing
- SfAutocomplete for recipient lookup
- SfChipGroup for quick filters and recipients
- SfAvatarView for sender visuals
- SfBadgeView for folder counts
- SfPopup for menus and transient messages

## Features demonstrated

- Mail folder navigation
- Message selection and reading pane updates
- Archive filtering
- Restore deleted mail to Inbox
- Empty Trash action
- Draft selection and editing
- Compose and send mail with recipient and subject validation
- Local mock data only

## Architecture overview

The sample uses MVVM with CommunityToolkit.Mvvm. Each screen has its own ViewModel. Mock data lives in memory in `MockData` and shared mailbox collections in `Mailbox`.

## Setup instructions

Open the solution in Visual Studio with the .NET MAUI workload and Syncfusion package access enabled. The project uses the package versions listed in the `.csproj` file.

## How to run

Use the MAUI app launch target for your platform in Visual Studio.

## Responsive behavior

The sample is designed for both desktop and mobile layouts. Desktop screens use multi-column panes. Mobile screens use single-column layouts and larger tap targets.

## Known limitations

- The sample uses local mock data only.
- There is no authentication or cloud sync.
- There is no backend API.
- The current sandbox does not include the Android SDK, so a full Android build cannot be verified here.
