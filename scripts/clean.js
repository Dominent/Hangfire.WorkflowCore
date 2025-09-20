#!/usr/bin/env node

const { spawn } = require('cross-spawn');
const chalk = require('chalk');
const path = require('path');
const fs = require('fs-extra');
const { program } = require('commander');

program
  .option('--deep', 'Deep clean including node_modules and packages')
  .option('--keep-packages', 'Keep generated NuGet packages')
  .parse();

const options = program.opts();

console.log(chalk.blue('🧹 Cleaning Hangfire.WorkflowCore...'));

async function runCommand(command, args, description) {
  return new Promise((resolve, reject) => {
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

async function removeDirectory(dirPath, description) {
  try {
    if (await fs.pathExists(dirPath)) {
      await fs.remove(dirPath);
      console.log(chalk.gray(`  🗑️  Removed ${dirPath}`));
    } else {
      console.log(chalk.gray(`  ⏭️  ${dirPath} not found`));
    }
  } catch (error) {
    console.log(chalk.red(`  ❌ Failed to remove ${dirPath}: ${error.message}`));
  }
}

async function cleanBuildArtifacts() {
  console.log(chalk.yellow('\n▶️  Cleaning build artifacts'));
  
  const artifactDirs = [
    'bin',
    'obj'
  ];

  for (const dir of artifactDirs) {
    await removeDirectory(dir, `Removing ${dir}`);
  }
}

async function cleanPackages() {
  if (!options.keepPackages) {
    console.log(chalk.yellow('\n▶️  Cleaning package outputs'));
    
    const packageDirs = [
      'dist',
      'release',
      'coverage'
    ];

    for (const dir of packageDirs) {
      await removeDirectory(dir, `Removing ${dir}`);
    }
  } else {
    console.log(chalk.yellow('\n⏭️  Keeping package outputs (--keep-packages specified)'));
  }
}

async function cleanNodeModules() {
  if (options.deep) {
    console.log(chalk.yellow('\n▶️  Deep cleaning Node.js dependencies'));
    await removeDirectory('node_modules', 'Removing node_modules');
    
    try {
      if (await fs.pathExists('package-lock.json')) {
        await fs.remove('package-lock.json');
        console.log(chalk.gray(`  🗑️  Removed package-lock.json`));
      }
    } catch (error) {
      console.log(chalk.red(`  ❌ Failed to remove package-lock.json: ${error.message}`));
    }
  }
}

async function cleanTemporaryFiles() {
  console.log(chalk.yellow('\n▶️  Cleaning temporary files'));
  
  const tempPatterns = [
    'TestResults',
    '*.tmp',
    '*.temp',
    '.vs'
  ];

  for (const pattern of tempPatterns) {
    try {
      // Use simple fs operations for common patterns
      if (pattern === 'TestResults' || pattern === '.vs') {
        await removeDirectory(pattern, `Removing ${pattern}`);
      }
    } catch (error) {
      // Ignore errors for patterns that might not exist
    }
  }
}

async function runDotnetClean() {
  try {
    await runCommand('dotnet', ['clean'], 'Running dotnet clean');
  } catch (error) {
    console.log(chalk.yellow('⚠️  dotnet clean failed (this is usually not critical)'));
  }
}

async function showCleanSummary() {
  console.log(chalk.green('\n🎉 Cleaning completed!'));
  
  // Show current directory size
  try {
    const { execSync } = require('child_process');
    const sizeCommand = process.platform === 'win32' 
      ? 'dir /s /-c | find "bytes"'
      : 'du -sh . 2>/dev/null || echo "Size calculation not available"';
    
    const size = execSync(sizeCommand, { encoding: 'utf8' }).trim();
    console.log(chalk.blue(`📊 Current directory size: ${size}`));
  } catch (e) {
    console.log(chalk.gray('📊 Directory size calculation not available'));
  }

  console.log(chalk.blue('\n💡 To restore the project:'));
  if (options.deep) {
    console.log(chalk.gray('  1. npm install'));
    console.log(chalk.gray('  2. npm run restore'));
    console.log(chalk.gray('  3. npm run build'));
  } else {
    console.log(chalk.gray('  1. npm run restore'));
    console.log(chalk.gray('  2. npm run build'));
  }
}

async function clean() {
  try {
    // Run dotnet clean first
    await runDotnetClean();
    
    // Clean build artifacts
    await cleanBuildArtifacts();
    
    // Clean packages (unless keeping them)
    await cleanPackages();
    
    // Clean temporary files
    await cleanTemporaryFiles();
    
    // Deep clean node modules if requested
    await cleanNodeModules();
    
    // Show summary
    await showCleanSummary();

  } catch (error) {
    console.error(chalk.red('\n💥 Cleaning failed!'));
    console.error(chalk.red(error.message));
    process.exit(1);
  }
}

clean();