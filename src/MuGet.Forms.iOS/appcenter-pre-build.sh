#!/bin/bash

plistPath="${BUILD_REPOSITORY_LOCALPATH}/src/MuGet.Forms.iOS/Info.plist"
appCsPath="${BUILD_REPOSITORY_LOCALPATH}/src/MuGet.Forms.UI/App.xaml.cs"
buildNumber=$APPCENTER_BUILD_ID
buildOffset=$BUILD_ID_OFFSET
buildShortVersion="$VERSION_NAME"

# exit if a command fails
set -e

if [ ! -f $plistPath ] ; then
  echo " [!] File doesn't exist at specified Info.plist path: ${plistPath}"
  exit 1
fi

if [ ! -f $appCsPath ] ; then
  echo " [!] File doesn't exist at specified App.xaml.cs path: ${appCsPath}"
  exit 1
fi

sed -i "" "s/APPCENTER_ANDROID/${APPCENTER_ANDROID}/g" "${appCsPath}"
sed -i "" "s/APPCENTER_IOS/${APPCENTER_IOS}/g" "${appCsPath}"

buildVersion=$((buildNumber + buildOffset))

if [ -z "$buildShortVersion" ] ; then
    buildShortVersion="$(date -u +'%Y.%-m.%-d')"
fi

echo " (i) Provided Info.plist path: ${plistPath}"

bundleVerCmd="/usr/libexec/PlistBuddy -c \"Print CFBundleVersion\" \"${plistPath}\""
bundleShortVerCmd="/usr/libexec/PlistBuddy -c \"Print CFBundleShortVersionString\" \"${plistPath}\""

# verbose / debug print commands
set -v

# ---- Current Bundle Version:
eval $bundleVerCmd
# ---- Set Bundle Version:
/usr/libexec/PlistBuddy -c "Set :CFBundleVersion ${buildVersion}" "${plistPath}"
# ---- New Bundle Version:
eval $bundleVerCmd

# ---- Current Bundle Short Version String:
eval $bundleShortVerCmd
# ---- Set Bundle Short Version String:
/usr/libexec/PlistBuddy -c "Set :CFBundleShortVersionString ${buildShortVersion}" "${plistPath}"
# ---- New Bundle Short Version String:
eval $bundleShortVerCmd

set +v