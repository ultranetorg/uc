#!/bin/python3

import os
import re


def gen_test_entry(filepath: str) -> str:
    filepath_tokens = filepath.split(os.path.sep)
    # Group 3 is where name will be captured.
    # Name is provided as a parameter in the test constructor, hence we capture
    # something like `: base("Wallet Import Account By Key")`
    pat_name_exp = re.compile(r'(base)(\s*\(\s*\")(.*)\"(\s*)(?=\))')

    # Test description is expected to be provided as a typical <summary> xml block.
    pat_description_exp = re.compile(r'(?<=<summary>)[\s\S]*?(?=<\/summary>)')

    out: str = "";
    test_name = ""
    test_desc = ""


    file = open(filepath, 'r', encoding="utf8")
    code = file.read()
    file.close()

    match = re.search(pat_name_exp, code)
    if match:
        test_name = "".join(match.group(3))

    match = re.search(pat_description_exp, code)
    if match:
        test_desc = "".join(match.group())

    # Remove triple dashes.
    test_desc = test_desc.replace("/// ", "")

    p = (os.path.join(filepath_tokens[-2], filepath_tokens[-1])).replace('\\', '/')
    out += f"### {test_name}\n"
    out += f"- File: `{p}`\n"
    if test_desc: out += f"- Description:  \n```{test_desc}```\n"
    out += "---"
    out += "\n"

    print(f"Generated entry for {p}.")
    return out


current_dir = os.path.dirname(os.path.realpath(__file__))
tests_dir = os.path.join(current_dir, "Tests")
doc = os.path.join(tests_dir, "README.md")


test_files: list[str] = []
tests_cmd: list[str] = []
tests_e2e: list[str] = []
tests_sets: list[str] = []


for root, _, files in os.walk(tests_dir):
    for f in files:
        if ".cs" in f:
            fullpath = os.path.join(root, f)
            test_files.append(fullpath)
            if "Cmd" in f: tests_cmd.append(fullpath)
            if "E2E" in f: tests_e2e.append(fullpath)
            if "Testset" in f: tests_sets.append(fullpath)


test_files.sort()
tests_cmd.sort()
tests_e2e.sort()
tests_sets.sort()


out = open(doc, 'w')

out.write("# Tests\n\n")

for f in test_files:
    entry = gen_test_entry(f)
    out.write(entry)

# out.write("## Command line tests\n\n")
# 
# for f in tests_cmd:
    # entry = gen_test_entry(f)
    # out.write(entry)
# 
# 
# out.write("## End-To-End Tests\n\n")
# 
# for f in tests_e2e:
    # entry = gen_test_entry(f)
    # out.write(entry)
# 
# 
# out.write("## Test-sets\n\n")
# 
# for f in tests_sets:
    # entry = gen_test_entry(f)
    # out.write(entry)

out.close()



