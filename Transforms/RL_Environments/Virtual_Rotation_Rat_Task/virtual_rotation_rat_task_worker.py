
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
    global observation_type

    if not get_parameters(_worker_object):
        return False

    if not cu.connect_sockets():
        return False

    if not cu.start_unity_exe(node_dir, game):
        return False

    if not cu.first_communication_with_unity(screen_res, translation_snap, rotation_snap, observation_type):
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
    command_type = command_and_typevalue[1].split(':')[0]
    command_value = command_and_typevalue[1].split(':')[1]

    # Send the action or parameter to Unity
    if command == 'Parameter':
        cu.change_parameter(command_type, command_value)
    if command == 'Action':
        cu.do_action(command_type, command_value)

    # Get the reward and observations from Unity
    reward, pixels, features, dt_of_frame = cu.get_observation(observation_type)
    print(dt_of_frame)

    # Generate the result
    pixels_features_reward_dict = {}
    result = [np.array([ct.IGNORE])]
    if features is not None:
        pixels_features_reward_dict = copy.copy(features)
    if pixels is not None:
        pixels_features_reward_dict['Pixels'] = np.ascontiguousarray(pixels).tolist()
    if reward is not None:
        pixels_features_reward_dict['Reward'] = reward

    if len(pixels_features_reward_dict) > 0:
        result = [pixels_features_reward_dict]
        #print(np.array(result[0]['Pixels']).shape)

    # Deal with the substate
    command_features_reward = copy.copy(pixels_features_reward_dict)
    if command == 'Parameter':
        command_features_reward['parameter'] = [command_type, command_value]
    if command == 'Action':
        command_features_reward['action'] = [command_type, command_value]
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
