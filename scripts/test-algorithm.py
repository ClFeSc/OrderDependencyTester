import argparse
import os


def generate_argparse() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        '--set-based-path',
        '-s',
        type=str,
        required=True,
    )
    parser.add_argument(
        '--invalid-path',
        '-i',
        type=str,
        required=True,
    )
    parser.add_argument(
        '--valid-path',
        '-v',
        type=str,
        required=True,
    )
    parser.add_argument(
        '--attributes-path',
        '-a',
        type=str,
        required=True,
    )
    return parser


def main():
    args = generate_argparse().parse_args()
    set_based, invalid, valid, attributes = args.set_based_path, args.invalid_path, args.valid_path, args.attributes_path
    # Test valids
    command = f'dotnet run -c Release --project CliFrontend {set_based} {valid} {attributes} | grep "is not valid"'
    result = os.system(command)
    if os.WEXITSTATUS(result) == 0:
        raise ValueError("Found at least one valid candidate that was reported as invalid!")
    # Test invalids
    command = f'dotnet run -c Release --project CliFrontend {set_based} {invalid} {attributes} | grep -v "is not valid"'
    result = os.system(command)
    if os.WEXITSTATUS(result) == 0:
        raise ValueError("Found at least one invalid candidate that was reported as valid!")


if __name__ == '__main__':
    main()
