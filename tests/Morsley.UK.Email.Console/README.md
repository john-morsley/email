Morsley.UK.Email.Console
========================

This simple program does the following...

1. Generates a GUID.
2. Sends 5 emails to [Data.ToAddress].
    a. Two 'blank' emails.
    b. One 'real email.
    c. Another two 'blank' emails.
3. Reads all emails in the inbox.
    a. Iterates through these emails.
    b. Displays each email.
    c. Highlights the 'real email.

The 'blank' emails look like:

- Subject = Morsley.UK.Email.Console - [YYYY-MM-DD HH:MM:SS:FFF]
- Text Body =

The 'real' email looks like:

- Subject = Morsley.UK.Email.Console - [Above GUID]
- Text Body = Unique - [Above GUID]

What is quite interesting is that the 'real' email is not always in the middle as you'd expect it to be!