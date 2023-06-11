
# The script MUST define a class named AzureMLModel.
# This class MUST at least define the following three methods: "__init__", "train" and "predict".
# The signatures (method and argument names) of all these methods MUST be exactly the same as the following example.

# Please do not install extra packages such as "pip install xgboost" in this script,
# otherwise errors will be raised when reading models in down-stream modules.


import pandas as pd
from sklearn.linear_model import LogisticRegression
from sklearn.ensemble import RandomForestClassifier

#Class contains machine learning model training
class AzureMLModel:
    # The __init__ method is only invoked in module "Create Python Model",
    # and will not be invoked again in the following modules "Train Model" and "Score Model".
    # The attributes defined in the __init__ method are preserved and usable in the train and predict method.
    def __init__(self):
        # self.model must be assigned

        self.model = RandomForestClassifier(n_estimators=100,max_depth=10,max_features=None,random_state=42)
        self.feature_column_names = ['cp','trestbps','chol','fbs','restecg','thalach','exang','oldpeak','slope','ca','thal']

    # Train model
    #   Param<df_train>: a pandas.DataFrame
    #   Param<df_label>: a pandas.Series
    def train(self, df_train, df_label):
        # self.feature_column_names records the column names used for training.
        # It is recommended to set this attribute before training so that the
        # feature columns used in predict and train methods have the same names.
        self.feature_column_names = df_train.columns.tolist()
        self.model.fit(df_train, df_label)

    # Predict results
    #   Param<df>: a pandas.DataFrame
    #   Must return a pandas.DataFrame
    def predict(self, df):
        # The feature columns used for prediction MUST have the same names as the ones for training.
        # The name of score column ("Scored Labels" in this case) MUST be different from any other
        # columns in input data.
        return pd.DataFrame({'Scored Labels': self.model.predict(df[self.feature_column_names])})



#this below function contains logic of extracting CustomerID column from dataset
def azureml_main(dataframe1 = None, dataframe2 = None):

    # Execution logic goes here
    #print(f'Input pandas.DataFrame #1: {dataframe1}')

    # If a zip file is connected to the third input port,
    # it is unzipped under "./Script Bundle". This directory is added
    # to sys.path. Therefore, if your zip file contains a Python file
    # mymodule.py you can import it using:
    # import mymodule

    # Return value must be of a sequence of pandas.DataFrame
    # E.g.
    #   -  Single return value: return dataframe1,
    #   -  Two return values: return dataframe1, dataframe2

    custId = dataframe1['CustomerId']
    custId.to_frame()
    return custId,



# The script MUST contain a function named azureml_main
# which is the entry point for this module.

# imports up here can be used to


# The entry point function MUST have two input arguments.
# If the input port is not connected, the corresponding
# dataframe argument will be None.
#   Param<dataframe1>: a pandas.DataFrame
#   Param<dataframe2>: a pandas.DataFrame


#this below function contains logic of combining CustomerID column to predcitons results
def azureml_main(dataframe1 = None, dataframe2 = None):

    # Execution logic goes here
    print(f'Input pandas.DataFrame #1: {dataframe1}')

    # If a zip file is connected to the third input port,
    # it is unzipped under "./Script Bundle". This directory is added
    # to sys.path. Therefore, if your zip file contains a Python file
    # mymodule.py you can import it using:
    # import mymodule

    # Return value must be of a sequence of pandas.DataFrame
    # E.g.
    #   -  Single return value: return dataframe1,
    #   -  Two return values: return dataframe1, dataframe2
    dataframe1['CustomerId'] = dataframe2.iloc[:,0]
    return dataframe1,

