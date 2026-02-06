#!/bin/bash

# Delete Unity private assets
git rm Assets/ModulesPrivate

# Update submodules
git submodule update --remote --init --recursive

# For more info, see: https://github.com/PavelZinchenko/event-horizon-main/issues/325#issuecomment-2043104776