
import sys
from os import path

current_dir = path.dirname(path.abspath(__file__))
node_dir = current_dir
while path.split(current_dir)[-1] != r'Heron':
    current_dir = path.dirname(current_dir)
sys.path.insert(0, path.dirname(current_dir))

import numpy as np
import copy
from Heron.communication.socket_for_serialization import Socket
from Heron import general_utils as gu, constants as ct
import commands_to_unity as cu

game: str
screen_res = []
observation_type: str
translation_snap: float
rotation_snap: int
initialised = False


def get_parameters(_worker_object):
    global game
    global screen_res
    global observation_type
    global translation_snap
    global rotation_snap

    try:
        parameters = _worker_object.parameters
        game = parameters[0]
        screen_res_x = parameters[1]
        screen_res_y = parameters[2]
        translation_snap = parameters[3]
        rotation_snap = parameters[4]
        observation_type = parameters[5]
    except:
        return False

    screen_res.append(screen_res_x)
    screen_res.append(screen_res_y)

    _worker_object.savenodestate_create_parameters_df(game=game, screen_res_x=screen_res_x,screen_res_y=screen_res_y,
                                                      translation_snap=translation_snap, rotation_snap=rotation_snap,
                                                      observation_type=observation_type)
    return True


def initialise(_worker_object):
    global initialised

    if not get_parameters(_worker_object):
        return False

    if not cu.connect_sockets():
        return False

    if not cu.start_unity_exe(node_dir, game):
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

    command_and_typevalue = message[0].split('=')
    command = command_and_typevalue[0]
    type = command_and_typevalue[1].split(':')[0]
    value = command_and_typevalue[1].split(':')[1]

    # Send the action to Unity
    if command == 'Parameter':
        cu.change_parameter(type, value)
    if command == 'Action':
        cu.do_action(type, value)

    # Get the reward and observations from Unity
    gu.accurate_delay(3)  # This delay is required so that the observation is the current one and to the previous one.
    reward, pixels, features, dt_of_frame = cu.get_observation(observation_type)

    print(dt_of_frame)

    # Generate the result
    result = [np.array([ct.IGNORE])]*2
    if pixels is not None:
        result[0] = np.ascontiguousarray(pixels)
    if features is not None:
        features_reward_dict = copy.copy(features)
    else:
        features_reward_dict = {}
    if reward is not None:
        features_reward_dict['Reward'] = reward

    if len(features_reward_dict) > 0:
        result[1] = features_reward_dict

    # Deal with the substate
    command_features_reward = copy.copy(features_reward_dict)
    if command == 'Parameter':
        command_features_reward['parameter'] = [type, value]
    if command == 'Action':
        command_features_reward['action'] = [type, value]
    savenodestate_update_substate_df(**command_features_reward)

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
