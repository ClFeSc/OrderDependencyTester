import argparse
import itertools
import re


def generate_argparse() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        '--path',
        '-p',
        type=str,
        required=True,
        help='The path where all candidates will be written to.'
    )
    return parser


def main():
    path = generate_argparse().parse_args().path
    columns = ["A","B","C","D"]

    possibleDirections = [list(itertools.product((True, False), repeat=i)) for i in range(len(columns) + 1)]

    def generate_all_sided_permutations(columns):
        for subset_length in range(1,len(columns) + 1):
            for subset in itertools.combinations(columns,subset_length):
                for permutation in itertools.permutations(subset):
                    for direction in possibleDirections[subset_length]:
                        yield permutation, direction


    def print_with_direction(col, col_direction):
        return ",".join([col + ("↑" if direction else "↓") for col, direction in zip(col, col_direction)])

    def print_setbased(lhs, lhsDirection, rhs, rhsDirection):
        return f"[{print_with_direction(lhs, lhsDirection)}] -> [{print_with_direction(rhs, rhsDirection)}]"


    with open(path,'w+')as f:
        for lhs, lhsDirection in generate_all_sided_permutations(columns):
            for rhs, rhsDirection in generate_all_sided_permutations(columns):
                f.write(print_setbased(lhs, lhsDirection, rhs, rhsDirection) + '\n')


if __name__ == '__main__':
    main()
