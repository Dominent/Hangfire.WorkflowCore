#!/usr/bin/env node

const fs = require('fs-extra');
const path = require('path');
const chalk = require('chalk');
const semver = require('semver');
const { program } = require('commander');

program
  .option('-t, --type <type>', 'Version bump type (major|minor|patch)', 'patch')
  .option('--version <version>', 'Specific version to set')
  .option('--dry-run', 'Show what would be changed without making changes')
  .parse();

const options = program.opts();

const buildPropsPath = path.resolve(__dirname, '..', 'Directory.Build.props');
const packageJsonPath = path.resolve(__dirname, '..', 'package.json');

async function getCurrentVersion() {
  try {
    const content = await fs.readFile(buildPropsPath, 'utf8');
    const versionMatch = content.match(/<Version>(.*?)<\/Version>/);
    if (versionMatch) {
      return versionMatch[1];
    }
  } catch (e) {
    console.log(chalk.yellow('Could not read current version from Directory.Build.props'));
  }
  return '1.0.0';
}

async function updateBuildProps(newVersion) {
  const content = await fs.readFile(buildPropsPath, 'utf8');
  
  // Convert semantic version to assembly version (X.Y.Z.0)
  const versionParts = newVersion.split('.');
  const assemblyVersion = `${versionParts[0]}.${versionParts[1] || '0'}.${versionParts[2] || '0'}.0`;
  
  let updatedContent = content
    .replace(/<Version>.*?<\/Version>/, `<Version>${newVersion}</Version>`)
    .replace(/<AssemblyVersion>.*?<\/AssemblyVersion>/, `<AssemblyVersion>${assemblyVersion}</AssemblyVersion>`)
    .replace(/<FileVersion>.*?<\/FileVersion>/, `<FileVersion>${assemblyVersion}</FileVersion>`);
  
  if (!options.dryRun) {
    await fs.writeFile(buildPropsPath, updatedContent);
  }
  
  return updatedContent !== content;
}

async function updatePackageJson(newVersion) {
  const packageJson = await fs.readJson(packageJsonPath);
  const oldVersion = packageJson.version;
  packageJson.version = newVersion;
  
  if (!options.dryRun) {
    await fs.writeJson(packageJsonPath, packageJson, { spaces: 2 });
  }
  
  return oldVersion !== newVersion;
}

async function bumpVersion() {
  try {
    const currentVersion = await getCurrentVersion();
    console.log(chalk.blue(`Current version: ${currentVersion}`));

    let newVersion;
    if (options.version) {
      if (!semver.valid(options.version)) {
        throw new Error(`Invalid version: ${options.version}`);
      }
      newVersion = options.version;
    } else {
      newVersion = semver.inc(currentVersion, options.type);
      if (!newVersion) {
        throw new Error(`Failed to increment version: ${currentVersion} (${options.type})`);
      }
    }

    console.log(chalk.green(`New version: ${newVersion}`));

    if (options.dryRun) {
      console.log(chalk.yellow('\n🔍 Dry run mode - no files will be changed'));
    } else {
      console.log(chalk.yellow('\n📝 Updating version in files...'));
    }

    // Update Directory.Build.props
    const buildPropsChanged = await updateBuildProps(newVersion);
    if (buildPropsChanged) {
      console.log(chalk.gray(`  ✅ Directory.Build.props ${options.dryRun ? '(would be updated)' : 'updated'}`));
    } else {
      console.log(chalk.gray(`  ⏭️  Directory.Build.props already up to date`));
    }

    // Update package.json
    const packageJsonChanged = await updatePackageJson(newVersion);
    if (packageJsonChanged) {
      console.log(chalk.gray(`  ✅ package.json ${options.dryRun ? '(would be updated)' : 'updated'}`));
    } else {
      console.log(chalk.gray(`  ⏭️  package.json already up to date`));
    }

    if (!options.dryRun) {
      console.log(chalk.green('\n🎉 Version updated successfully!'));
      console.log(chalk.blue('\n💡 Next steps:'));
      console.log(chalk.gray('  1. npm run build'));
      console.log(chalk.gray('  2. npm run test'));
      console.log(chalk.gray('  3. npm run pack:nuget'));
      console.log(chalk.gray('  4. git add . && git commit -m "Bump version to v' + newVersion + '"'));
      console.log(chalk.gray('  5. git tag v' + newVersion));
    }

    return newVersion;

  } catch (error) {
    console.error(chalk.red('\n💥 Version bump failed!'));
    console.error(chalk.red(error.message));
    process.exit(1);
  }
}

// If this script is run directly, execute the version bump
if (require.main === module) {
  bumpVersion().then(version => {
    if (!options.dryRun) {
      console.log(chalk.green(`\n✨ Version is now ${version}`));
    }
  });
}

module.exports = { bumpVersion, getCurrentVersion };