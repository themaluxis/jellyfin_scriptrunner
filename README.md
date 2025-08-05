# Jellyfin Shell Executor Plugin

A simple Jellyfin plugin that executes shell scripts on server initialization with a web-based configuration interface.

## Features

- 🚀 Execute shell scripts automatically when Jellyfin starts
- 📝 Web-based script editor integrated into Jellyfin's plugin settings
- 🔧 No manual file editing required - configure everything through the UI
- 📊 Script output logged to Jellyfin logs
- 🐧 Cross-platform support (Linux, macOS, Windows with WSL)

## Installation

### Method 1: Manual Installation

1. Download the latest release from the [Releases](https://github.com/yourusername/JellyfinShellExecutor/releases) page
2. Extract the ZIP file to your Jellyfin plugins directory:
   - Linux: `/var/lib/jellyfin/plugins/` or `/config/plugins/`
   - Windows: `%ProgramData%\Jellyfin\Server\plugins\`
   - Docker: `/config/plugins/`
3. Restart Jellyfin

### Method 2: Build from Source

#### Prerequisites
- .NET 8.0 SDK or later
- Git

#### Build Steps
```bash
# Clone the repository
git clone https://github.com/yourusername/JellyfinShellExecutor.git
cd JellyfinShellExecutor

# Build the plugin
dotnet build -c Release

# Package the plugin (Linux/macOS)
./package.sh

# Or manually copy files
cp JellyfinShellExecutor/bin/Release/net8.0/* /path/to/jellyfin/plugins/JellyfinShellExecutor-1.0.0.0/
```

## Configuration

1. Navigate to **Dashboard** → **Plugins** → **Shell Executor** in your Jellyfin web interface
2. Edit the script content in the text area
3. Click **Save** to update the script
4. The script will execute automatically on the next Jellyfin restart

### Default Script
```bash
#!/bin/bash
# Default script content
echo "Shell Executor Plugin initialized at $(date)"
```

## Usage Examples

### Example 1: Backup Database on Startup
```bash
#!/bin/bash
BACKUP_DIR="/backup/jellyfin"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)

# Create backup directory if it doesn't exist
mkdir -p "$BACKUP_DIR"

# Backup Jellyfin database
cp /var/lib/jellyfin/data/jellyfin.db "$BACKUP_DIR/jellyfin_${TIMESTAMP}.db"

echo "Database backed up to $BACKUP_DIR/jellyfin_${TIMESTAMP}.db"
```

### Example 2: Mount Network Drives
```bash
#!/bin/bash
# Mount network shares on startup
mount -t cifs //192.168.1.100/media /mnt/media -o credentials=/root/.smbcredentials
mount -t cifs //192.168.1.100/backup /mnt/backup -o credentials=/root/.smbcredentials

echo "Network drives mounted successfully"
```

### Example 3: Update Custom Libraries
```bash
#!/bin/bash
# Update external media database
/opt/scripts/update_media_db.sh

# Sync metadata from external source
rsync -av --delete /mnt/metadata/ /var/lib/jellyfin/metadata/

echo "Custom libraries updated"
```

### Example 4: System Health Check
```bash
#!/bin/bash
# Check disk space
DISK_USAGE=$(df -h / | awk 'NR==2 {print $5}' | sed 's/%//')

if [ $DISK_USAGE -gt 90 ]; then
    echo "WARNING: Disk usage is at ${DISK_USAGE}%"
    # Send notification (requires configured notification system)
    # curl -X POST http://localhost:8123/api/notify -d "message=Jellyfin disk space critical"
fi

# Check if critical services are running
services=("nginx" "redis" "postgresql")
for service in "${services[@]}"; do
    if ! systemctl is-active --quiet $service; then
        echo "WARNING: $service is not running"
    fi
done
```

## Logs

Script output and errors are logged to the Jellyfin log file. Check logs at:
- Linux: `/var/log/jellyfin/`
- Docker: View with `docker logs <container_name>`
- Windows: `%ProgramData%\Jellyfin\Server\logs\`

Example log entries:
```
[13:37:42] [INF] [1] JellyfinShellExecutor.Plugin: Script output: Database backed up successfully
[13:37:42] [INF] [1] JellyfinShellExecutor.Plugin: Script executed with exit code: 0
```

## Security Considerations

⚠️ **Warning**: This plugin executes shell scripts with the same privileges as the Jellyfin server process.

- Only install scripts from trusted sources
- Review all scripts before execution
- Consider using restricted user accounts for Jellyfin
- Be cautious with scripts that accept user input
- Avoid storing sensitive credentials directly in scripts

## Troubleshooting

### Plugin won't load
- Ensure you're using a compatible Jellyfin version (10.9.0 or later)
- Check that all plugin files are in the correct directory
- Verify file permissions (Jellyfin user must have read access)

### Script not executing
- Check Jellyfin logs for error messages
- Ensure the script has proper shebang (`#!/bin/bash`)
- Verify script syntax: `bash -n /path/to/script.sh`
- Test script manually: `sudo -u jellyfin /path/to/script.sh`

### Permission denied errors
```bash
# Make script executable
chmod +x /config/plugins/JellyfinShellExecutor-1.0.0.0/script.sh

# Fix ownership
chown -R jellyfin:jellyfin /config/plugins/JellyfinShellExecutor-1.0.0.0/
```

## Platform-Specific Notes

### Docker
When running Jellyfin in Docker, ensure any paths referenced in your scripts are accessible within the container:
```yaml
volumes:
  - /host/backup:/backup
  - /host/scripts:/scripts
```

### Windows
For Windows systems, modify the plugin to use PowerShell:
- Scripts should use `.ps1` extension
- Update the executor to use `powershell.exe` instead of `/bin/bash`

### macOS
Ensure Terminal has full disk access if accessing protected directories:
- System Preferences → Security & Privacy → Privacy → Full Disk Access

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Jellyfin](https://jellyfin.org/) team for the amazing media server
- [Jellyfin Plugin Template](https://github.com/jellyfin/jellyfin-plugin-template) for the foundation

## Support

- Create an [Issue](https://github.com/yourusername/JellyfinShellExecutor/issues) for bug reports or feature requests
- Check [Jellyfin Documentation](https://jellyfin.org/docs/) for general Jellyfin questions
- Join [Jellyfin Discord](https://discord.gg/jellyfin) for community support