# Lore Manual

*Placeholder: the full manual follows before the first public release.*

## The Basics

- **Project**: your world. One Lore Project per Unity project.
- **Categories** group your **Entries**, e.g. Characters, Places or Items.
- **Fields** describe what every Entry of a Category has, e.g. Home, Health or Portrait.
- **Tags** mark Entries across Categories, e.g. Witch or Companion.

## Descriptions

Type `[[` in a description to link another Entry. Links stay intact when you rename the Entry.

Descriptions use Markdown emphasis: two asterisks or underscores around a text make it bold, one makes it italic.
While you edit, the markers stay visible.

## Health and Integrity

- Yellow marks unfinished content, e.g. empty required values.
- Red marks broken data, e.g. references to deleted Entries.

## Using Lore in Your Game

Read your Project at runtime through the read-only API. See the Runtime API documentation in the package.
