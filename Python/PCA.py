# PCA.py


import os
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.decomposition import PCA
from sklearn.preprocessing import StandardScaler


import os
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
from sklearn.decomposition import PCA
from sklearn.preprocessing import StandardScaler

def pca_plot(
    input_file: str,
    color_variable: str = "Group",
    shape_variable: str = None,
    show_sample_names: bool = True,
    plot_title: str = "PCA Plot",
    file_name: str = "pca_plot",

    # --- Customizations ---
    sample_name_fontsize: int = 9,
    sample_name_offset: tuple = (0.01, 0.01),

    point_alpha: float = 0.7,
    point_size: int = 100,

    title_fontsize: int = 18,
    axis_label_fontsize: int = 14,
    tick_fontsize: int = 12,
    legend_fontsize: int = 12,
    legend_title_fontsize: int = 14,

    fig_width: int = 10,
    fig_height: int = 8,
    dpi_value: int = 300,
    show_grid: bool = True,
    grid_style: str = "--",
    grid_alpha: float = 0.3,

    # --- New argument ---
    exclude_samples: list[str] = None,
):
    """
    Perform PCA with optional exclusion of specific samples.
    Returns:
        (png_path, used_samples, all_samples)
    """

    if not os.path.exists(input_file):
        raise FileNotFoundError(f"Input file not found: {input_file}")

    # --- Step 1: Read raw file (no headers, no index)
    _, ext = os.path.splitext(input_file)
    if ext.lower() == ".csv":
        raw = pd.read_csv(input_file, header=None)
    elif ext.lower() in [".xls", ".xlsx"]:
        raw = pd.read_excel(input_file, header=None)
    else:
        raise ValueError("Input file must be CSV or Excel")

    # --- Step 2: Extract sample IDs from row 0, cols 1..end
    sample_ids = raw.iloc[0, 1:].tolist()
    all_samples = sample_ids[:]   # keep a full copy

    # --- Step 3: Extract Group and Condition
    groups = raw.iloc[1, 1:].tolist()
    has_condition = False
    conditions = None
    data_start_row = 2

    if raw.shape[0] > 2 and str(raw.iloc[2, 0]).strip().lower() == "condition":
        has_condition = True
        conditions = raw.iloc[2, 1:].tolist()
        data_start_row = 3

    # --- Step 4: Extract feature matrix
    feature_names = raw.iloc[data_start_row:, 0].tolist()
    numeric_part = raw.iloc[data_start_row:, 1:]

    # ensure numeric conversion
    values = pd.to_numeric(numeric_part.stack(), errors="coerce").unstack().values.T
    if pd.isna(values).any():
        raise ValueError("Non-numeric values detected in feature rows.")

    # Build metadata DataFrame (aligned with samples)
    metadata = pd.DataFrame({
        "SampleID": sample_ids,
        "Group": groups
    })
    if has_condition:
        metadata["Condition"] = conditions

    # Feature matrix with SampleIDs as index
    features = pd.DataFrame(values, index=sample_ids, columns=feature_names)

    # --- Step 5: Exclude unwanted samples BEFORE merging
    if exclude_samples:
        metadata = metadata[~metadata["SampleID"].isin(exclude_samples)]
        features = features.loc[metadata["SampleID"]]  # align rows properly

    used_samples = metadata["SampleID"].tolist()

    # Combine metadata + features
    data = pd.concat([metadata.reset_index(drop=True),
                      features.reset_index(drop=True)], axis=1)

    metadata_cols = ["SampleID", "Group"] + (["Condition"] if has_condition else [])

    # --- Step 6: PCA
    X = data.drop(columns=metadata_cols).values
    X_scaled = StandardScaler().fit_transform(X)
    pca = PCA(n_components=2)
    components = pca.fit_transform(X_scaled)

    components_df = pd.DataFrame(components, columns=["PC1", "PC2"])
    components_df = pd.concat([data[metadata_cols], components_df], axis=1)

    # --- Step 7: Output folder = same as input
    save_dir = os.path.dirname(os.path.abspath(input_file))
    os.makedirs(save_dir, exist_ok=True)

    # --- Step 8: Plotting
    plt.figure(figsize=(fig_width, fig_height), dpi=dpi_value)
    marker_shapes = ["o", "s", "D", "^", "v", "P", "X"]

    if color_variable and color_variable in components_df.columns:
        unique_colors = components_df[color_variable].unique()
        palette = sns.color_palette("Set2", n_colors=len(unique_colors))
    else:
        unique_colors = ["All"]
        palette = sns.color_palette("Set2", n_colors=1)

    for idx, cgroup in enumerate(unique_colors):
        subset = components_df if color_variable is None else components_df[components_df[color_variable] == cgroup]

        if shape_variable and shape_variable in components_df.columns:
            unique_shapes = subset[shape_variable].unique()
            for jdx, cond in enumerate(unique_shapes):
                cond_subset = subset[subset[shape_variable] == cond]
                plt.scatter(
                    cond_subset["PC1"], cond_subset["PC2"],
                    label=f"{cgroup}-{cond}",
                    c=[palette[idx]],
                    alpha=point_alpha, s=point_size,
                    marker=marker_shapes[jdx % len(marker_shapes)]
                )
                if show_sample_names:
                    for _, row in cond_subset.iterrows():
                        plt.text(
                            row["PC1"] + sample_name_offset[0],
                            row["PC2"] + sample_name_offset[1],
                            row["SampleID"],
                            fontsize=sample_name_fontsize,
                            alpha=0.8
                        )
        else:
            plt.scatter(
                subset["PC1"], subset["PC2"],
                label=cgroup,
                c=[palette[idx]],
                alpha=point_alpha, s=point_size
            )
            if show_sample_names:
                for _, row in subset.iterrows():
                    plt.text(
                        row["PC1"] + sample_name_offset[0],
                        row["PC2"] + sample_name_offset[1],
                        row["SampleID"],
                        fontsize=sample_name_fontsize,
                        alpha=0.8
                    )

    plt.xlabel(f"PC1 ({pca.explained_variance_ratio_[0]*100:.1f}% variance)",
               fontsize=axis_label_fontsize)
    plt.ylabel(f"PC2 ({pca.explained_variance_ratio_[1]*100:.1f}% variance)",
               fontsize=axis_label_fontsize)
    plt.title(plot_title, fontsize=title_fontsize)

    leg = plt.legend()
    plt.setp(leg.get_texts(), fontsize=legend_fontsize)
    plt.setp(leg.get_title(), fontsize=legend_title_fontsize)

    plt.xticks(fontsize=tick_fontsize)
    plt.yticks(fontsize=tick_fontsize)
    if show_grid:
        plt.grid(True, linestyle=grid_style, alpha=grid_alpha)
    else:
        plt.grid(False)

    plt.tight_layout()

    png_path = os.path.join(save_dir, f"{file_name}.png")
    pdf_path = os.path.join(save_dir, f"{file_name}.pdf")
    csv_path = os.path.join(save_dir, f"{file_name}.csv")

    # --- Delete old files if they exist ---
    for f in [png_path, pdf_path, csv_path]:
        if os.path.exists(f):
            os.remove(f)

    # --- Save new outputs ---
    plt.savefig(png_path, format="png", bbox_inches="tight")
    plt.savefig(pdf_path, format="pdf", bbox_inches="tight")
    components_df.to_csv(csv_path, index=False)
    plt.close()

    return png_path, used_samples, all_samples




def pca_plot_Working(
    input_file: str,
    color_variable: str = "Group",
    shape_variable: str = None,
    show_sample_names: bool = True,
    plot_title: str = "PCA Plot",
    file_name: str = "pca_plot",

    # --- Customizations ---
    sample_name_fontsize: int = 9,
    sample_name_offset: tuple = (0.01, 0.01),

    point_alpha: float = 0.7,
    point_size: int = 100,

    title_fontsize: int = 18,
    axis_label_fontsize: int = 14,
    tick_fontsize: int = 12,
    legend_fontsize: int = 12,
    legend_title_fontsize: int = 14,

    fig_width: int = 10,
    fig_height: int = 8,
    dpi_value: int = 300,
    show_grid: bool = True,
    grid_style: str = "--",
    grid_alpha: float = 0.3,
):
    """
    Perform PCA when:
      - First column = row labels (Group / Condition / feature names)
      - First row = sample IDs (header row, cols 1..N)
      - Second row = 'Group' assignments
      - Third row (optional) = 'Condition'
      - Remaining rows = numeric feature values
    """

    if not os.path.exists(input_file):
        raise FileNotFoundError(f"Input file not found: {input_file}")

    # --- Step 1: Read raw file (no headers, no index)
    _, ext = os.path.splitext(input_file)
    if ext.lower() == ".csv":
        raw = pd.read_csv(input_file, header=None)
    elif ext.lower() in [".xls", ".xlsx"]:
        raw = pd.read_excel(input_file, header=None)
    else:
        raise ValueError("Input file must be CSV or Excel")

    # --- Step 2: Extract sample IDs from row 0, cols 1..end
    sample_ids = raw.iloc[0, 1:].tolist()

    # --- Step 3: Extract Group and Condition
    groups = raw.iloc[1, 1:].tolist()
    has_condition = False
    conditions = None
    data_start_row = 2

    if raw.shape[0] > 2 and str(raw.iloc[2, 0]).strip().lower() == "condition":
        has_condition = True
        conditions = raw.iloc[2, 1:].tolist()
        data_start_row = 3

    # --- Step 4: Extract feature matrix
    feature_names = raw.iloc[data_start_row:, 0].tolist()
    numeric_part = raw.iloc[data_start_row:, 1:]

    # ensure numeric conversion
    values = pd.to_numeric(numeric_part.stack(), errors="coerce").unstack().values.T
    if pd.isna(values).any():
        raise ValueError(
            "Non-numeric values detected in feature rows. "
            "Please check your input file."
        )

    # Build dataframe
    data = pd.DataFrame(values, columns=feature_names)
    data.insert(0, "SampleID", sample_ids)
    data.insert(1, "Group", groups)
    if has_condition:
        data.insert(2, "Condition", conditions)

    metadata_cols = ["SampleID", "Group"] + (["Condition"] if has_condition else [])

    # --- Step 5: PCA
    X = data.drop(columns=metadata_cols).values
    X_scaled = StandardScaler().fit_transform(X)
    pca = PCA(n_components=2)
    components = pca.fit_transform(X_scaled)

    components_df = pd.DataFrame(components, columns=["PC1", "PC2"])
    components_df = pd.concat([data[metadata_cols], components_df], axis=1)

    # --- Step 6: Output folder = same as input
    save_dir = os.path.dirname(os.path.abspath(input_file))
    os.makedirs(save_dir, exist_ok=True)

    # --- Step 7: Plotting
    plt.figure(figsize=(fig_width, fig_height), dpi=dpi_value)
    marker_shapes = ["o", "s", "D", "^", "v", "P", "X"]

    if color_variable and color_variable in components_df.columns:
        unique_colors = components_df[color_variable].unique()
        palette = sns.color_palette("Set2", n_colors=len(unique_colors))
    else:
        unique_colors = ["All"]
        palette = sns.color_palette("Set2", n_colors=1)

    for idx, cgroup in enumerate(unique_colors):
        subset = (
            components_df if color_variable is None
            else components_df[components_df[color_variable] == cgroup]
        )

        if shape_variable and shape_variable in components_df.columns:
            unique_shapes = subset[shape_variable].unique()
            for jdx, cond in enumerate(unique_shapes):
                cond_subset = subset[subset[shape_variable] == cond]
                plt.scatter(
                    cond_subset["PC1"],
                    cond_subset["PC2"],
                    label=f"{cgroup}-{cond}",
                    c=[palette[idx]],
                    alpha=point_alpha,
                    s=point_size,
                    marker=marker_shapes[jdx % len(marker_shapes)]
                )
                if show_sample_names:
                    for _, row in cond_subset.iterrows():
                        plt.text(
                            row["PC1"] + sample_name_offset[0],
                            row["PC2"] + sample_name_offset[1],
                            row["SampleID"],
                            fontsize=sample_name_fontsize,
                            alpha=0.8
                        )
        else:
            plt.scatter(
                subset["PC1"],
                subset["PC2"],
                label=cgroup,
                c=[palette[idx]],
                alpha=point_alpha,
                s=point_size
            )
            if show_sample_names:
                for _, row in subset.iterrows():
                    plt.text(
                        row["PC1"] + sample_name_offset[0],
                        row["PC2"] + sample_name_offset[1],
                        row["SampleID"],
                        fontsize=sample_name_fontsize,
                        alpha=0.8
                    )

    # Axis labels with variance explained
    plt.xlabel(f"PC1 ({pca.explained_variance_ratio_[0]*100:.1f}% variance)",
               fontsize=axis_label_fontsize)
    plt.ylabel(f"PC2 ({pca.explained_variance_ratio_[1]*100:.1f}% variance)",
               fontsize=axis_label_fontsize)

    # Title
    plt.title(plot_title, fontsize=title_fontsize)

    # Legend
    leg = plt.legend()
    plt.setp(leg.get_texts(), fontsize=legend_fontsize)
    plt.setp(leg.get_title(), fontsize=legend_title_fontsize)

    # Ticks
    plt.xticks(fontsize=tick_fontsize)
    plt.yticks(fontsize=tick_fontsize)

    # Grid
    if show_grid:
        plt.grid(True, linestyle=grid_style, alpha=grid_alpha)
    else:
        plt.grid(False)

    plt.tight_layout()

    # --- Step 8: Save outputs (same as before)
    png_path = os.path.join(save_dir, f"{file_name}.png")
    pdf_path = os.path.join(save_dir, f"{file_name}.pdf")
    csv_path = os.path.join(save_dir, f"{file_name}.csv")

    plt.savefig(png_path, format="png", bbox_inches="tight")
    plt.savefig(pdf_path, format="pdf", bbox_inches="tight")
    components_df.to_csv(csv_path, index=False)
    plt.close()

    return png_path



def pca(
    data: pd.DataFrame = None,
    input_file: str = None,
    metadata_cols: list = None,
    group_variable: str = "Group",
    shape_variable: str = "Condition",
    show_sample_names: bool = True,
    sample_name_fontsize: int = 9,
    sample_name_offset: tuple = (0.01, 0.01),
    color_palette: str = "Set2",
    marker_shapes: list = None,
    point_alpha: float = 0.7,
    point_size: int = 100,
    title_fontsize: int = 18,
    axis_label_fontsize: int = 14,
    tick_fontsize: int = 12,
    legend_fontsize: int = 12,
    legend_title_fontsize: int = 14,
    font_family: str = "Arial",
    plot_title: str = "PCA Plot",
    show_variance: bool = True,
    fig_width: int = 10,
    fig_height: int = 8,
    dpi_value: int = 300,
    show_grid: bool = True,
    grid_style: str = "--",
    grid_alpha: float = 0.3,
    save_figure: bool = True
):
    """
    Create a customizable PCA plot. If no data is provided, synthetic data will be generated.
    Always saves PCA outputs in the folder where the script is located, under PCA_Plot/.
    """

    # =============================
    # STEP 1: HANDLE DATA
    # =============================
    if data is None:
        # generate synthetic dataset
        np.random.seed(42)
        n_samples, n_features = 40, 50
        sample_ids = [f"Sample_{i+1}" for i in range(n_samples)]
        groups = np.random.choice(["Control", "Treatment"], size=n_samples)
        conditions = np.random.choice(["Low", "Medium", "High"], size=n_samples)
        features = np.random.randn(n_samples, n_features) * 5 + np.arange(n_features)

        columns = [f"Feature_{j+1}" for j in range(n_features)]
        data = pd.DataFrame(features, columns=columns)
        data.insert(0, "Condition", conditions)
        data.insert(0, "Group", groups)
        data.insert(0, "SampleID", sample_ids)

    if metadata_cols is None:
        metadata_cols = ["SampleID", "Group", "Condition"]

    if marker_shapes is None:
        marker_shapes = ["o", "s", "D", "^", "v", "P", "X"]

    # =============================
    # STEP 2: PCA
    # =============================
    feature_cols = [col for col in data.columns if col not in metadata_cols]
    X = data[feature_cols].values
    X_scaled = StandardScaler().fit_transform(X)

    pca = PCA(n_components=2)
    components = pca.fit_transform(X_scaled)

    components_df = pd.DataFrame(components, columns=["PC1", "PC2"])
    components_df = pd.concat([data[metadata_cols], components_df], axis=1)

    # =============================
    # STEP 3: OUTPUT DIRECTORY (where script is located)
    # =============================
    save_dir = os.path.join(os.path.dirname(os.path.abspath(__file__)), "PCA_Plot")
    if save_figure:
        os.makedirs(save_dir, exist_ok=True)

    # =============================
    # STEP 4: PLOTTING
    # =============================
    plt.figure(figsize=(fig_width, fig_height), dpi=dpi_value)

    unique_groups = components_df[group_variable].unique()
    palette = sns.color_palette(color_palette, n_colors=len(unique_groups))

    for idx, group in enumerate(unique_groups):
        subset = components_df[components_df[group_variable] == group]
        if shape_variable:
            unique_shapes = subset[shape_variable].unique()
            for jdx, cond in enumerate(unique_shapes):
                cond_subset = subset[subset[shape_variable] == cond]
                plt.scatter(
                    cond_subset["PC1"],
                    cond_subset["PC2"],
                    label=f"{group}-{cond}",
                    c=[palette[idx]],
                    alpha=point_alpha,
                    s=point_size,
                    marker=marker_shapes[jdx % len(marker_shapes)]
                )
                if show_sample_names:
                    for _, row in cond_subset.iterrows():
                        plt.text(
                            row["PC1"] + sample_name_offset[0],
                            row["PC2"] + sample_name_offset[1],
                            row["SampleID"],
                            fontsize=sample_name_fontsize,
                            fontfamily=font_family,
                            alpha=0.8
                        )
        else:
            plt.scatter(
                subset["PC1"],
                subset["PC2"],
                label=group,
                c=[palette[idx]],
                alpha=point_alpha,
                s=point_size
            )
            if show_sample_names:
                for _, row in subset.iterrows():
                    plt.text(
                        row["PC1"] + sample_name_offset[0],
                        row["PC2"] + sample_name_offset[1],
                        row["SampleID"],
                        fontsize=sample_name_fontsize,
                        fontfamily=font_family,
                        alpha=0.8
                    )

    # axis labels
    x_label = "PC1"
    y_label = "PC2"
    if show_variance:
        x_label += f" ({pca.explained_variance_ratio_[0]*100:.1f}% variance)"
        y_label += f" ({pca.explained_variance_ratio_[1]*100:.1f}% variance)"

    plt.title(plot_title, fontsize=title_fontsize, fontfamily=font_family)
    plt.xlabel(x_label, fontsize=axis_label_fontsize, fontfamily=font_family)
    plt.ylabel(y_label, fontsize=axis_label_fontsize, fontfamily=font_family)

    plt.xticks(fontsize=tick_fontsize, fontfamily=font_family)
    plt.yticks(fontsize=tick_fontsize, fontfamily=font_family)

    if show_grid:
        plt.grid(True, linestyle=grid_style, alpha=grid_alpha)

    plt.legend(
        title=f"{group_variable} / {shape_variable}",
        fontsize=legend_fontsize,
        title_fontsize=legend_title_fontsize
    )
    plt.tight_layout()

    # =============================
    # STEP 5: SAVE OUTPUTS
    # =============================
    if save_figure:
        png_path = os.path.join(save_dir, "PCA_plot.png")
        pdf_path = os.path.join(save_dir, "PCA_plot.pdf")
        csv_path = os.path.join(save_dir, "PCA_data.csv")

        plt.savefig(png_path, format="png", bbox_inches="tight")
        plt.savefig(pdf_path, format="pdf", bbox_inches="tight")
        components_df.to_csv(csv_path, index=False)

        print(f"PCA plot saved to:\n {png_path}\n {pdf_path}")
        print(f"PCA data saved to:\n {csv_path}")
    else:
        plt.show()

    plt.close()
    return save_dir

