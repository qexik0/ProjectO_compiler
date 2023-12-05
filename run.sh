#!/bin/bash

if [ -z "$1" ]; then
    echo "You should provide a file."
    exit 1
fi

dotnet run -- semantic-report --input $1 --output=analized_result.txt
llc -filetype=obj output.ll -o output.o -opaque-pointers
clang -c olang_lib.c -o olang_lib.o
clang output.o olang_lib.o -o output -lm
./output
