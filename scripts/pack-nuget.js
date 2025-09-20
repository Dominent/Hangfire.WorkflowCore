#!/usr/bin/env node

const { spawn } = require('cross-spawn');
const chalk = require('chalk');
const path = require('path');
const fs = require('fs-extra');
const { program } = require('commander');

program
  .option('-c, --configuration <config>', 'Pack configuration (Debug|Release)', 'Release')
  .option('-v, --version <version>', 'Package version override')
  .option('--include-symbols', 'Include symbol packages')
  .option('--output <dir>', 'Output directory for packages', './dist')
  .parse();

const options = program.opts();

console.log(chalk.blue('📦 Packing Hangfire.WorkflowCore NuGet packages...'));
console.log(chalk.gray(`Configuration: ${options.configuration}`));

const projects = [
  'src/Hangfire.WorkflowCore.Abstractions/Hangfire.WorkflowCore.Abstractions.csproj',
  'src/Hangfire.WorkflowCore/Hangfire.WorkflowCore.csproj',
  'src/Hangfire.WorkflowCore.Dashboard/Hangfire.WorkflowCore.Dashboard.csproj',
  'src/Hangfire.WorkflowCore.AspNetCore/Hangfire.WorkflowCore.AspNetCore.csproj'
];

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

async function ensureOutputDir() {
  await fs.ensureDir(options.output);
  console.log(chalk.gray(`Output directory: ${path.resolve(options.output)}`));
}

async function packProject(projectPath) {
  const projectName = path.basename(projectPath, '.csproj');
  
  const packArgs = [
    'pack',
    projectPath,
    '--configuration', options.configuration,
    '--output', options.output,
    '--no-restore',
    '--no-build'
  ];

  if (options.version) {
    packArgs.push('-p:PackageVersion=' + options.version);
  }

  if (options.includeSymbols) {
    packArgs.push('--include-symbols');
    packArgs.push('-p:SymbolPackageFormat=snupkg');
  }

  await runCommand('dotnet', packArgs, `Packing ${projectName}`);
}

async function copyPackagesToDist() {
  // Copy packages from bin directories to dist
  const { execSync } = require('child_process');
  try {
    console.log(chalk.yellow('\n▶️  Copying packages to dist directory'));
    
    // Find all .nupkg files in bin directories
    const findCommand = process.platform === 'win32' 
      ? 'dir /s /b bin\\*.nupkg'
      : 'find bin -name "*.nupkg" -type f';
    
    const packages = execSync(findCommand, { encoding: 'utf8' })
      .split('\n')
      .filter(line => line.trim());

    for (const pkg of packages) {
      if (pkg.trim()) {
        const filename = path.basename(pkg);
        const destPath = path.join(options.output, filename);
        await fs.copy(pkg.trim(), destPath);
        console.log(chalk.gray(`  📄 ${filename}`));
      }
    }
  } catch (e) {
    console.log(chalk.gray('  (No additional packages found in bin directories)'));
  }
}

async function showPackageInfo() {
  try {
    const packages = await fs.readdir(options.output);
    const nupkgFiles = packages.filter(f => f.endsWith('.nupkg'));
    
    if (nupkgFiles.length > 0) {
      console.log(chalk.blue('\n📦 Generated packages:'));
      for (const pkg of nupkgFiles) {
        const stats = await fs.stat(path.join(options.output, pkg));
        const sizeKB = Math.round(stats.size / 1024);
        console.log(chalk.green(`  📄 ${pkg} (${sizeKB} KB)`));
      }
    }
  } catch (e) {
    console.log(chalk.gray('  (Package information not available)'));
  }
}

async function pack() {
  try {
    await ensureOutputDir();
    
    // Pack each project
    for (const project of projects) {
      await packProject(project);
    }

    // Copy any additional packages
    await copyPackagesToDist();
    
    // Show package information
    await showPackageInfo();

    console.log(chalk.green('\n🎉 NuGet packages created successfully!'));
    console.log(chalk.blue(`📁 Packages are available in: ${path.resolve(options.output)}`));
    
    if (!options.version) {
      console.log(chalk.yellow('\n💡 Tip: Use --version to specify a custom package version'));
    }

  } catch (error) {
    console.error(chalk.red('\n💥 Packaging failed!'));
    console.error(chalk.gray('Make sure to build the solution first with: npm run build'));
    process.exit(1);
  }
}

pack();