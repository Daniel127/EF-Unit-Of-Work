#!/bin/bash
function check_version() {
  if [[ $1 =~ ^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(-((0|[1-9][0-9]*|[0-9]*[a-zA-Z-][0-9a-zA-Z-]*)(\.(0|[1-9][0-9]*|[0-9]*[a-zA-Z-][0-9a-zA-Z-]*))*))?(\+([0-9a-zA-Z-]+(\.[0-9a-zA-Z-]+)*))?$ ]]; then
    echo "$1"
  else
    echo ""
  fi
}

echo "Configuring package version"

commitId=$BUILD_SOURCEVERSION

gitTagVersion="$(git describe --exact-match $commitId --abbrev=0 --tags --first-parent)"
appVersion=${gitTagVersion:1}
echo "Git version: $gitTagVersion"
echo "App version: $appVersion"

if [[ ! $(check_version ${appVersion}) ]]; then
  exit 1;
fi

echo "##vso[task.setvariable variable=packageVersion]$appVersion"
echo "Finished"