#!/usr/bin/env node

const { spawn } = require('cross-spawn');
const chalk = require('chalk');
const path = require('path');
const fs = require('fs-extra');
const { program } = require('commander');
const { getCurrentVersion } = require('./version-bump');

program
  .option('--skip-tests', 'Skip running tests before release')
  .option('--skip-build', 'Skip building before release')
  .option('--dry-run', 'Show what would happen without actually releasing')
  .option('-o, --output <dir>', 'Output directory for release packages', './release')
  .parse();

const options = program.opts();

console.log(chalk.blue('🚀 Preparing Hangfire.WorkflowCore Release...'));

async function runCommand(command, args, description, skipOnDryRun = false) {
  return new Promise((resolve, reject) => {
    if (options.dryRun && skipOnDryRun) {
      console.log(chalk.yellow(`\n🔍 [DRY RUN] Would run: ${description}`));
      resolve();
      return;
    }

    console.log(chalk.yellow(`\n▶️  ${description}`));
    
    const child = spawn(command, args, {
      stdio: 'inherit',
      cwd: path.resolve(__dirname, '..'),
      shell: process.platform === 'win32'
    });

    child.on('close', (code) => {
      if (code === 0) {
        console.log(chalk.green(`✅ ${description} completed successfully`));
        resolve();
      } else {
        console.log(chalk.red(`❌ ${description} failed with exit code ${code}`));
        reject(new Error(`${description} failed`));
      }
    });
  });
}

async function checkGitStatus() {
  try {
    const { execSync } = require('child_process');
    const status = execSync('git status --porcelain', { encoding: 'utf8' });
    
    if (status.trim()) {
      console.log(chalk.yellow('\n⚠️  Warning: You have uncommitted changes:'));
      console.log(chalk.gray(status));
      
      if (!options.dryRun) {
        const readline = require('readline').createInterface({
          input: process.stdin,
          output: process.stdout
        });
        
        const answer = await new Promise(resolve => {
          readline.question('Continue anyway? (y/N): ', resolve);
        });
        
        readline.close();
        
        if (answer.toLowerCase() !== 'y') {
          console.log(chalk.red('Release cancelled'));
          process.exit(1);
        }
      }
    }
  } catch (e) {
    console.log(chalk.yellow('⚠️  Could not check git status (git not available or not a git repo)'));
  }
}

async function createReleaseDirectory() {
  await fs.ensureDir(options.output);
  await fs.emptyDir(options.output);
  console.log(chalk.gray(`Release directory: ${path.resolve(options.output)}`));
}

async function copyReleaseAssets() {
  const assets = [
    'README.md',
    'Directory.Build.props',
    'Hangfire.WorkflowCore.sln'
  ];

  console.log(chalk.yellow('\n▶️  Copying release assets'));
  
  for (const asset of assets) {
    if (await fs.pathExists(asset)) {
      await fs.copy(asset, path.join(options.output, asset));
      console.log(chalk.gray(`  📄 ${asset}`));
    }
  }
}

async function copyPackages() {
  console.log(chalk.yellow('\n▶️  Copying NuGet packages'));
  
  // Copy from both bin and dist directories
  const packageSources = ['bin', 'dist'];
  
  for (const source of packageSources) {
    if (await fs.pathExists(source)) {
      const { execSync } = require('child_process');
      try {
        const findCommand = process.platform === 'win32' 
          ? `dir /s /b ${source}\\*.nupkg`
          : `find ${source} -name "*.nupkg" -type f`;
        
        const packages = execSync(findCommand, { encoding: 'utf8' })
          .split('\n')
          .filter(line => line.trim());

        for (const pkg of packages) {
          if (pkg.trim()) {
            const filename = path.basename(pkg);
            const destPath = path.join(options.output, 'packages', filename);
            await fs.ensureDir(path.dirname(destPath));
            await fs.copy(pkg.trim(), destPath);
            console.log(chalk.gray(`  📦 ${filename}`));
          }
        }
      } catch (e) {
        // Ignore if no packages found
      }
    }
  }
}

async function generateReleaseNotes() {
  const version = await getCurrentVersion();
  const releaseNotes = `# Hangfire.WorkflowCore v${version}

## Release Information

- **Version**: ${version}
- **Release Date**: ${new Date().toISOString().split('T')[0]}
- **Build Configuration**: Release

## Packages Included

- \`Hangfire.WorkflowCore.${version}.nupkg\` - Main library
- \`Hangfire.WorkflowCore.Abstractions.${version}.nupkg\` - Abstractions package

## Installation

\`\`\`bash
dotnet add package Hangfire.WorkflowCore --version ${version}
dotnet add package Hangfire.WorkflowCore.Abstractions --version ${version}
\`\`\`

## Features

- 🚀 Seamless integration between Hangfire and WorkflowCore
- 📅 Flexible workflow scheduling (immediate, delayed, recurring, continuation)
- 🔄 Advanced workflow orchestration capabilities
- 🛡️ Reliable execution with comprehensive error handling
- 🏗️ Clean architecture with well-defined abstractions
- 📊 Production-ready with logging and monitoring support

## Quick Start

See the [README.md](README.md) for complete usage instructions and examples.

## Sample Applications

- **VideoProcessing** - Demonstrates video processing workflows with multi-step orchestration

## Requirements

- .NET 6.0 or higher
- Hangfire.Core 1.8.14+
- WorkflowCore 3.10.0+

---

For detailed documentation and examples, visit the project repository.
`;

  const releaseNotesPath = path.join(options.output, 'RELEASE_NOTES.md');
  await fs.writeFile(releaseNotesPath, releaseNotes);
  console.log(chalk.gray(`  📝 RELEASE_NOTES.md`));
}

async function showReleaseSummary() {
  const version = await getCurrentVersion();
  
  console.log(chalk.green('\n🎉 Release preparation completed!'));
  console.log(chalk.blue(`\n📦 Release v${version} Summary:`));
  
  try {
    const releaseFiles = await fs.readdir(options.output, { withFileTypes: true });
    
    for (const file of releaseFiles) {
      if (file.isFile()) {
        const stats = await fs.stat(path.join(options.output, file.name));
        const sizeKB = Math.round(stats.size / 1024);
        console.log(chalk.gray(`  📄 ${file.name} (${sizeKB} KB)`));
      } else if (file.isDirectory()) {
        const dirFiles = await fs.readdir(path.join(options.output, file.name));
        console.log(chalk.gray(`  📁 ${file.name}/ (${dirFiles.length} files)`));
      }
    }
    
    console.log(chalk.blue(`\n📁 Release available in: ${path.resolve(options.output)}`));
    
  } catch (e) {
    console.log(chalk.gray('  (Release summary not available)'));
  }
}

async function release() {
  try {
    const version = await getCurrentVersion();
    console.log(chalk.gray(`Target version: ${version}`));
    
    if (options.dryRun) {
      console.log(chalk.yellow('\n🔍 DRY RUN MODE - No actual changes will be made\n'));
    }

    // Check git status
    await checkGitStatus();

    // Create release directory
    if (!options.dryRun) {
      await createReleaseDirectory();
    }

    // Build (unless skipped)
    if (!options.skipBuild) {
      await runCommand('npm', ['run', 'build:release'], 'Building solution');
    }

    // Test (unless skipped)
    if (!options.skipTests) {
      await runCommand('npm', ['run', 'test:all'], 'Running tests');
    }

    // Create packages
    await runCommand('npm', ['run', 'pack:nuget'], 'Creating NuGet packages', true);

    if (!options.dryRun) {
      // Copy release assets
      await copyReleaseAssets();
      
      // Copy packages
      await copyPackages();
      
      // Generate release notes
      await generateReleaseNotes();
      
      // Show summary
      await showReleaseSummary();
    }

    console.log(chalk.green(`\n✨ Release v${version} is ready!`));
    
    if (!options.dryRun) {
      console.log(chalk.blue('\n💡 Next steps:'));
      console.log(chalk.gray('  1. Review the release in: ' + path.resolve(options.output)));
      console.log(chalk.gray('  2. git add . && git commit -m "Release v' + version + '"'));
      console.log(chalk.gray('  3. git tag v' + version));
      console.log(chalk.gray('  4. git push origin main --tags'));
      console.log(chalk.gray('  5. Publish packages to NuGet:'));
      console.log(chalk.gray('     dotnet nuget push "' + path.resolve(options.output, 'packages') + '/*.nupkg" --source https://api.nuget.org/v3/index.json'));
    }

  } catch (error) {
    console.error(chalk.red('\n💥 Release failed!'));
    console.error(chalk.red(error.message));
    process.exit(1);
  }
}

release();