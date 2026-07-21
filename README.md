# LibraryManagement

Simple console-based library management system.

Architecture:
- Onion architecture with Core, DataAccess, Services, and App layers.

Authentication rules:
- Registration requires a unique email; usernames may be non-unique.
- Login is by email only.

Configuration:
- appsettings.json contains an "Email" section bound to LibraryManagement.Core.Configuration.EmailSettings.

Notes:
- Email sending falls back to logging when SMTP settings are incomplete or send fails.
- After changes, ensure appsettings.json has valid Email configuration if you want real email delivery.
