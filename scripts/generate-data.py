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
    seed: int | None = args.seed
    # Seed
    if seed is None:
        random.seed(seed)
        seed = int.from_bytes(random.randbytes(8), 'little')
    print(f'Used seed: {seed}')
    random.seed(seed)
    with open(output_path, mode = 'w') as file:
        for i in range(4):
            values = [random.randint(1, 4), random.randint(1, 4), random.randint(1, 4), random.randint(1, 4)]
            file.write(';'.join([str(x) for x in values]))
            file.write('\n')


if __name__ == '__main__':
    main()
