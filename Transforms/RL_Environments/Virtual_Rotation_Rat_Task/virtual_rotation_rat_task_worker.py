import profile
import sys
from os import path

current_dir = path.dirname(path.abspath(__file__))
node_dir = current_dir
while path.split(current_dir)[-1] != r'Heron':
    current_dir = path.dirname(current_dir)
sys.path.insert(0, path.dirname(current_dir))

import numpy as np
from functools import reduce
from Heron.communication.socket_for_serialization import Socket
from Heron import general_utils as gu, constants as ct
from Heron.gui.visualisation_dpg import VisualisationDPG
import commands_to_unity as cu


visualisation_dpg: VisualisationDPG
game: str
screen_res = []
observation_type: str
translation_snap: float
rotation_snap: int
initialised = False
reward_history = []
size_of_arena = 5


def get_parameters(_worker_object):
    global visualisation_dpg
    global game
    global screen_res
    global observation_type
    global translation_snap
    global rotation_snap

    try:
        parameters = _worker_object.parameters
        game = parameters[1]
        screen_res_x = parameters[2]
        screen_res_y = parameters[3]
        translation_snap = parameters[4]
        rotation_snap = parameters[5]
        observation_type = parameters[6]
    except:
        return False

    screen_res.append(screen_res_x)
    screen_res.append(screen_res_y)

    visualisation_dpg = VisualisationDPG(_node_name=_worker_object.node_name, _node_index=_worker_object.node_index,
                                         _visualisation_type='Single Pane Plot', _buffer=100,
                                         _x_axis_label='Latest Actions',
                                         _y_axis_base_label='Cumulative Reward',
                                         _base_plot_title='Cumulative Reward over Actions')

    visualisation_dpg.visualisation_on = parameters[0]

    _worker_object.savenodestate_create_parameters_df(visualisation_on=visualisation_dpg.visualisation_on, game=game,
                                                      screen_res_x=screen_res_x,screen_res_y=screen_res_y,
                                                      translation_snap=translation_snap, rotation_snap=rotation_snap,
                                                      observation_type=observation_type)
    return True


def update_reward_buffer_for_vis(reward):
    global reward_history
    reward = float(reward)

    if len(reward_history) > 1:
        reward_history.append(reward_history[-1] + reward)
    else:
        reward_history.append(reward)
    if len(reward_history) > 100:
        reward_history.pop(0)
    if visualisation_dpg.visualisation_on:
        visualisation_dpg.visualise(np.array(reward_history))


def vectorise_features(features):
    """
    Turns the features dictionary into a vector where each value is a normalised feature
    :param features: the dictionary of features coming from Unity
    :return: The normalised features vector
    """
    global game

    result_vector = []
    for p in features['Rat Position']:
        result_vector.append(p / size_of_arena + 0.5)
    result_vector.append(features['Rat Rotation'][0] / 360)
    if 'FindReward' not in game and 'ExploreCorners' not in game:
        result_vector.append(1 if features['Target Trap State'][0] else -1)
        result_vector.append(features['Manipulandum Angle'][0] / 360)
    if 'Buttons' in game:
        result_vector.append(1 if features['Left Paw Extended'][0] else -1)
        result_vector.append(1 if features['Right Paw Extended'][0] else -1)
    return result_vector


def generate_discrete_state(vectorised_features):
    global translation_snap
    global rotation_snap
    global game

    number_of_bins_per_dimension = [int(np.ceil(size_of_arena / translation_snap)),
                                    int(np.ceil(size_of_arena / translation_snap)),
                                    int(np.ceil(360 / rotation_snap))]
    if 'FindReward' not in game and 'ExploreCorners' not in game:
        number_of_bins_per_dimension.append(2)
        number_of_bins_per_dimension.append(360)
    if 'Buttons' in game:
        number_of_bins_per_dimension.append(2)
        number_of_bins_per_dimension.append(2)

    total_num_of_states = 1
    state_index = int(0)
    for i, s in enumerate(vectorised_features):
        state_index = state_index + int(np.ceil(s * reduce(np.multiply, np.array(number_of_bins_per_dimension[:(i+1)]),
                                                           1)))
        total_num_of_states *= number_of_bins_per_dimension[i]

    return state_index, total_num_of_states


def initialise(_worker_object):
    global initialised
    global observation_type

    if not get_parameters(_worker_object):
        return False

    if not cu.connect_sockets():
        return False

    if not cu.start_unity_exe(node_dir, game):
        return False
    gu.accurate_delay(4000)

    if not cu.first_communication_with_unity(screen_res, translation_snap, rotation_snap, observation_type):
        return False

    initialised = True

    return True


def work_function(data, parameters, savenodestate_update_substate_df):
    global observation_type
    global visualisation_dpg

    visualisation_dpg.visualisation_on = parameters[0]
    result = np.array([ct.IGNORE])

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
    #print(dt_of_frame)

    # Generate the result
    pixels_features_reward_dict = {}
    command_features_reward = {}
    result = [np.array([ct.IGNORE])]

    if features is not None and len(features) != 0:
        vectorised_features = vectorise_features(features)
        state_index, total_num_of_states = generate_discrete_state(vectorised_features)
        pixels_features_reward_dict['State Index'] = state_index

        command_features_reward = features
        command_features_reward['state index'] = state_index

    if pixels is not None:
        pixels_features_reward_dict['Pixels'] = np.ascontiguousarray(pixels).tolist()

    if reward is not None:
        pixels_features_reward_dict['Reward'] = reward
        update_reward_buffer_for_vis(reward)
    if len(pixels_features_reward_dict) > 0:
        result = [pixels_features_reward_dict]

    # Deal with the substate
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
