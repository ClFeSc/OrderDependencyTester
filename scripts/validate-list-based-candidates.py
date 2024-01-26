import argparse
from typing import Any, Sequence
import pandas as pd
import re
from tqdm import tqdm


def generate_argparse() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        '--input-file',
        '-i',
        type=str,
        required=True
    )
    parser.add_argument(
        '--valid-file',
        type=str,
        required=True,
    )
    parser.add_argument(
        '--invalid-file',
        type=str,
        required=True,
    )
    parser.add_argument(
        '--candidate-file-path',
        '-c',
        type=str,
        required=True,
        help='Path of a file containing all candidates to validate.'
    )
    return parser


listbased_matcher = re.compile(r"\[(.+)\] -> \[(.+)\]")


def number_to_excel_column(n: int):
    result = ""
    while n > 0:
        n, remainder = divmod(n - 1, 26)
        result = chr(65 + remainder) + result  # 65 is the ASCII code for 'A'
    return result


def excel_column_to_number(column_str: str) -> int:
    result = 0
    for i, char in enumerate(reversed(column_str)):
        result += (ord(char) - 64) * (26 ** i)  # 64 is the ASCII code for 'A'
    return result


def unique_counts(x: pd.Series):
    return len(x.unique())


def colsToString(cols: Sequence[int], directions: Sequence[bool]):
    return f",".join([number_to_excel_column(col + 1) + ("↑" if direction else "↓") for col, direction in zip(cols, directions)])
    # return f",".join([col + ("↑" if direction else "↓") for col, direction in zip(cols, directions)])


class ListBasedDependency():
    def __init__(self, df, lhs, lhsDirection, rhs, rhsDirection) -> None:
        self.df: pd.DataFrame = df
        self.df = pd.concat([self.df, self.df.isna().add_suffix("_isna")],axis=1)
        self.lhs: list[int] = lhs
        self.lhsDirection: list[bool] = lhsDirection
        self.rhs: list[int] = rhs
        self.rhsDirection: list[bool] = rhsDirection

    # ensure propoer null first sorting by first sorting by _isna columns
    # returns (columns, direction)
    def create_sort_args(self, columns, direction):
        sort_cols = []
        sort_directions = []
        for col, dir in zip(columns, direction):
            sort_cols.append(col + "_isna")
            sort_directions.append(True)
            sort_cols.append(col)
            sort_directions.append(dir)
        return sort_cols, sort_directions
    
    @staticmethod 
    def parse_attrlist(alist: str | Any):
         orderspecs = alist.split(',')
         attributes = [excel_column_to_number(spec[:-1]) - 1 for spec in orderspecs]
         directions = [ spec[-1] == '↑' for spec in orderspecs]
         return attributes, directions
    
    @staticmethod 
    def from_string(df: pd.DataFrame, s: str):
        match = listbased_matcher.match(s)
        if match is None:
            raise ValueError("No match found.")
        lhs, rhs = match.groups()
        lhs, lhsDirections = ListBasedDependency.parse_attrlist(lhs)
        rhs, rhsDirections = ListBasedDependency.parse_attrlist(rhs)
        return ListBasedDependency(df, lhs, lhsDirections, rhs, rhsDirections)

    def isValid(self):
        # no splits
        df_fd_check = self.df.groupby(self.lhs,dropna=False).agg({col: unique_counts for col in self.rhs})
        if not (df_fd_check == 1).all(axis=None):
             return False
        
        # no swaps
        lhs, lhsDirection = self.create_sort_args(self.lhs, self.lhsDirection)
        sorted_by_lhs = self.df.sort_values(lhs, ascending=lhsDirection)
        rhs, rhsDirection = self.create_sort_args(self.rhs, self.rhsDirection)
        sorted_by_rhs = sorted_by_lhs.sort_values(rhs, ascending=rhsDirection,kind="stable")
        return (sorted_by_lhs.index == sorted_by_rhs.index).all()
    
    def __str__(self):
        result = "["
        result += colsToString(self.lhs, self.lhsDirection)
        result += "] -> ["
        result += colsToString(self.rhs, self.rhsDirection)
        result += "]"
        return result


def validate_ods(input_path: str, valid_path: str, invalid_path: str, candidate_path: str):
    df = pd.read_csv(input_path,sep=";",header=None,keep_default_na=False, na_values=['', 'null','?'])
    with open(valid_path, 'w') as valid_file:
        with open(invalid_path, 'w') as invalid_file:
            with open(candidate_path, 'r') as candidate_file:
                for line in tqdm(candidate_file.readlines()):
                    lb = ListBasedDependency.from_string(df, line)
                    isValid = lb.isValid()
                    if isValid:
                        valid_file.write(str(lb))
                        valid_file.write('\n')
                    else:
                        invalid_file.write(str(lb))
                        invalid_file.write('\n')


def main():
    args = generate_argparse().parse_args()
    input_path = args.input_file
    valid_path = args.valid_file
    invalid_path = args.invalid_file
    candidate_path = args.candidate_file_path
    validate_ods(input_path, valid_path, invalid_path, candidate_path)


if __name__ == '__main__':
    main()
