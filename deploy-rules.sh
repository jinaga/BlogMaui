#! /bin/bash

dotnet build -c Release -f net8.0-maccatalyst

dotnet jinaga deploy authorization './BlogMaui/bin/Release/net8.0-maccatalyst/maccatalyst-arm64/BlogMaui.dll' $JINAGA_BLOG_AUTHORIZATION_URL $JINAGA_BLOG_SECRET
dotnet jinaga deploy distribution './BlogMaui/bin/Release/net8.0-maccatalyst/maccatalyst-arm64/BlogMaui.dll' $JINAGA_BLOG_DISTRIBUTION_URL $JINAGA_BLOG_SECRET