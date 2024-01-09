import argparse
import random


def generate_argparse() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        '--output-file',
        '-o',
        type=str,
        required=True,
    )
    parser.add_argument(
        '--seed',
        '-s',
        type=int,
        required=False,
        help='The seed to be used. If unspecified, seed is random.'
    )
    return parser


def main():
    args = generate_argparse().parse_args()
    output_path = args.output_file
    seed = args.seed
    # Seed
    random.seed(seed)
    with open(output_path, mode = 'w') as file:
        for i in range(4):
            values = [random.randint(1, 4), random.randint(1, 4), random.randint(1, 4), random.randint(1, 4)]
            file.write(';'.join([str(x) for x in values]))
            file.write('\n')


if __name__ == '__main__':
    main()
