#!/usr/bin/env bash

apt-get update
curl -sL https://deb.nodesource.com/setup_10.x -o nodesource_setup.sh
bash nodesource_setup.sh
apt-get install nodejs -y 