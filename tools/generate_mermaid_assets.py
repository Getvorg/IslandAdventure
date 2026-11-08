"""Compatibility entry point for the sprite build. Requires ImageMagick + bash."""
from pathlib import Path
import subprocess
subprocess.run(["bash", str(Path(__file__).with_name("build_mermaid_assets.sh"))], check=True)
