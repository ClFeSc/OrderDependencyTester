import argparse
import subprocess


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
    command = f'dotnet run -c Release --project CliFrontend {set_based} {valid} {attributes}'
    result = subprocess.check_output(command, shell=True)
    total_tested = int(result.splitlines()[-2].decode().split(': ')[-1])
    total_valid = int(result.splitlines()[-1].decode().split(': ')[-1])
    if total_valid != total_tested:
        raise ValueError(f"{total_valid} of {total_tested} ODs were valid, expected {total_tested}.")
    # Test invalids
    command = f'dotnet run -c Release --project CliFrontend {set_based} {invalid} {attributes}'
    result = subprocess.check_output(command, shell=True)
    total_tested = int(result.splitlines()[-2].decode().split(': ')[-1])
    total_valid = int(result.splitlines()[-1].decode().split(': ')[-1])
    if total_valid > 0:
        raise ValueError(f"{total_valid} of {total_tested} ODs were valid, expected 0.")


if __name__ == '__main__':
    main()
