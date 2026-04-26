import subprocess
import os
import sys

for f in filter(lambda f: not os.path.isdir(os.path.join(sys.argv[1], f)), os.listdir(sys.argv[1])):
    print("Test: " + f)
    subprocess.run(["dotnet", "run", "--", os.path.join(sys.argv[1], f)])
