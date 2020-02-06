#!/bin/bash

manifestPath="${BUILD_REPOSITORY_LOCALPATH}/src/MuGet.Forms.Android/Properties/AndroidManifest.xml"
appCsPath="${BUILD_REPOSITORY_LOCALPATH}/src/MuGet.Forms/App.xaml.cs"
buildNumber=$APPCENTER_BUILD_ID
buildOffset=$BUILD_ID_OFFSET
newVersionName="$VERSION_NAME"

# exit if a command fails
set -e

if [ ! -f $manifestPath ] ; then
  echo " [!] File doesn't exist at specified AndroidManifest.xml path: ${manifestPath}"
  exit 1
fi

if [ ! -f $appCsPath ] ; then
  echo " [!] File doesn't exist at specified App.xaml.cs path: ${appCsPath}"
  exit 1
fi

sed -i "" "s/APPCENTER_ANDROID/${APPCENTER_ANDROID}/g" "${appCsPath}"
sed -i "" "s/APPCENTER_IOS/${APPCENTER_IOS}/g" "${appCsPath}"

verCodeCmd="grep versionCode ${manifestPath} | sed 's/.*versionCode=\"//;s/\".*//'"
verNameCmd="grep versionName ${manifestPath} | sed 's/.*versionName\s*=\s*\"\([^\"]*\)\".*/\1/g'"

versionCode=$(eval $verCodeCmd)
versionName=$(eval $verNameCmd)

newVersionCode=$((buildNumber + buildOffset))

if [ -z "$newVersionName" ] ; then
    newVersionName="$(date -u +'%Y.%-m.%-d')"
fi

echo " (i) Provided AndroidManifest.xml path: ${manifestPath}"

# verbose / debug print commands
set -v

# ---- Current Version Code:
echo $versionCode
# ---- Set Version Code:
sed -i.bak "s/android:versionCode="\"${versionCode}\""/android:versionCode="\"${newVersionCode}\""/" ${manifestPath}
# ---- New Version Code:
eval $verCodeCmd

# ---- Current Version Name:
echo $versionName
# ---- Set Version Name:
sed -i.bak "s/android:versionName="\"${versionName}\""/android:versionName="\"${newVersionName}\""/" ${manifestPath}
# ---- New Version Name:
eval $verNameCmd

# ==> Manifest Version Code and Name changed