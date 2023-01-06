
import sys
from os import path

current_dir = path.dirname(path.abspath(__file__))
node_dir = current_dir
while path.split(current_dir)[-1] != r'Heron':
    current_dir = path.dirname(current_dir)
sys.path.insert(0, path.dirname(current_dir))

import numpy as np
from Heron.communication.socket_for_serialization import Socket
from Heron import general_utils as gu, constants as ct
import commands_to_unity as cu


screen_res = []
observation_type: str
translation_snap: float
rotation_snap: int
initialised = False


def get_parameters(_worker_object):
    global screen_res
    global observation_type
    global translation_snap
    global rotation_snap

    try:
        parameters = _worker_object.parameters
        screen_res_x = parameters[0]
        screen_res_y = parameters[1]
        translation_snap = parameters[2]
        rotation_snap = parameters[3]
        observation_type = parameters[4]
    except:
        return False

    screen_res.append(screen_res_x)
    screen_res.append(screen_res_y)

    return True


def initialise(_worker_object):
    global initialised

    if not get_parameters(_worker_object):
        return False

    if not cu.connect_sockets():
        return False

    if not cu.start_unity_exe(node_dir):
        return False

    if not cu.first_communication_with_unity(screen_res, translation_snap, rotation_snap):
        return False
    
    initialised = True

    return True


def work_function(data, parameters, savenodestate_update_substate_df):

    global observation_type

    topic = data[0]

    message = data[1:]  # data[0] is the topic
    message = Socket.reconstruct_data_from_bytes_message(message)

    action_type = message[0].split(',')[0].replace(' ', '')
    action_value = message[0].split(',')[1].replace(' ', '')
    
    
    cu.do_action(action_type, action_value)

    # savenodestate_update_substate_df(image__shape=message.shape)

    gu.accurate_delay(30)
    reward, pixels, features, dt_of_frame = cu.get_observation(observation_type)
    
    result = [np.array([ct.IGNORE])]*3
    if reward is not None:
        result[0] = np.array([reward])
    if pixels is not None:
        result[1] = np.ascontiguousarray(pixels)
    if features is not None:
        result[2] = features

    return result


# The on_end_of_life function must exist even if it is just a pass
def on_end_of_life():
    if initialised:
        cu.kill_unity()


if __name__ == "__main__":
    worker_object = gu.start_the_transform_worker_process(work_function=work_function,
                                                          end_of_life_function=on_end_of_life,
                                                          initialisation_function=initialise)
    worker_object.start_ioloop()
