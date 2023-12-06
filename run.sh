#!/bin/bash

if [ -z "$1" ]; then
    echo "You should provide a file."
    exit 1
fi

dotnet run -- semantic-report --input $1 --output=analized_result.txt
llc -filetype=obj output.ll -o output.o -opaque-pointers
clang -shared output.o -o liboutput.so

clang -c olang_lib.c -o olang_lib.o
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:.
clang olang_lib.o -L. -loutput -lavcall
./output Program 1 2 3
