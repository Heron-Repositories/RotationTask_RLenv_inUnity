import os
import sys
from os import path

current_dir = path.dirname(path.abspath(__file__))
while path.split(current_dir)[-1] != r'Heron':
    current_dir = path.dirname(current_dir)
sys.path.insert(0, path.dirname(current_dir))

from Heron import general_utils as gu
Exec = os.path.abspath(__file__)
# </editor-fold>

# <editor-fold desc="The following code is called from the GUI process as part of the generation of the node.
# It is meant to create node specific elements (not part of a generic node).
# This is where a new node's individual elements should be defined">
"""
Properties of the generated Node
"""
BaseName = 'Virtual Rotation Rat Task'   # The base name can have spaces.
NodeAttributeNames = ['Parameters', 'Action', 'Reward and Observations Dict']

NodeAttributeType = ['Static', 'Input', 'Output']

ParameterNames = ['Visualisation', 'Game', 'Screen Resolution X', 'Screen Resolution Y', 'Translation Snap', 'Rotation Snap',
                  'Observation Type']
ParameterTypes = ['bool', 'str', 'int', 'int', 'float', 'int', 'list']

ParametersDefaultValues = [False, 'TTM_FindReward', 100, 100, 0.1, 10, ['Pixels', 'Features', 'Everything']]

# The following line needs to exist with the correct name for the xxx_worker.py script
WorkerDefaultExecutable = os.path.join(os.path.dirname(Exec), 'virtual_rotation_rat_task_worker.py')
# </editor-fold>


# <editor-fold desc="The following code is called as its own process when the editor starts the graph.
#  You can refactor the name of the xxx_com variable but do not change anything else">
if __name__ == "__main__":
    virtual_rotation_rat_task_com = gu.start_the_transform_communications_process(NodeAttributeType, NodeAttributeNames)
    gu.register_exit_signals(virtual_rotation_rat_task_com.on_kill)
    virtual_rotation_rat_task_com.start_ioloop()

# </editor-fold>